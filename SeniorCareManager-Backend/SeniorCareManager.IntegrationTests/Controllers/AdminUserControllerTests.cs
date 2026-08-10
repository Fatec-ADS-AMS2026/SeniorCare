using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminUserControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminUserControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WithoutPermission_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db);

        var client = _factory.CreateClient().AsUser(userId);
        var response = await client.GetAsync("/api/v1/AdminUser");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_ThenGet_CreatesProvisionedAccountInCallerInstitution()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);

        var client = _factory.CreateClient().AsUser(adminId);
        var email = $"nova-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/v1/AdminUser",
            new AdminUserCreateRequest { Email = email, DisplayName = "Pessoa Nova" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<AdminUserDTO>();
        created!.AccountState.Should().Be(AccountState.PROVISIONED);

        using var assertScope = _factory.Services.CreateScope();
        var assertDb = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await assertDb.Users.SingleAsync(u => u.Id == created.Id);
        stored.InstitutionId.Should().Be(institutionId);
    }

    [Fact]
    public async Task Post_DoesNotIncludeActivationTokenInResponse()
    {
        // platform-authentication: "Senhas, códigos MFA, tokens de ativação, recuperação
        // e sessão SHALL NOT aparecer em... respostas administrativas" — sem ressalva
        // para a resposta de criação. O token existe (AdminUserService.CreateAsync o
        // gera), mas o controller nunca o repassa pra resposta HTTP.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);

        var client = _factory.CreateClient().AsUser(adminId);
        var email = $"nova-{Guid.NewGuid():N}@example.com";
        var createResponse = await client.PostAsJsonAsync("/api/v1/AdminUser",
            new AdminUserCreateRequest { Email = email, DisplayName = "Pessoa Nova" });
        var raw = await createResponse.Content.ReadAsStringAsync();

        raw.Should().NotContain("activationToken", "a resposta de criação não deve expor o token de ativação");
        raw.Should().NotContain("ActivationToken", "a resposta de criação não deve expor o token de ativação");
    }

    [Fact]
    public async Task ChangeState_WrongCurrentPassword_Denies()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var targetId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUser/{targetId}/state",
            new AdminUserStateChangeRequest { AccountState = AccountState.INACTIVE, CurrentPassword = "senha-errada" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangeState_LastActiveAdministrator_IsBlocked()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUser/{adminId}/state",
            new AdminUserStateChangeRequest { AccountState = AccountState.INACTIVE, CurrentPassword = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ChangeState_SecondAdministratorPresent_Succeeds()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var (_, secondAdminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        // Reaproveita a mesma instituição para o segundo admin (o helper cria uma nova por
        // padrão) — move manualmente.
        using (var fixScope = _factory.Services.CreateScope())
        {
            var fixDb = fixScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var second = await fixDb.Users.SingleAsync(u => u.Id == secondAdminId);
            second.InstitutionId = institutionId;
            await fixDb.SaveChangesAsync();
        }

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUser/{secondAdminId}/state",
            new AdminUserStateChangeRequest { AccountState = AccountState.INACTIVE, CurrentPassword = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
