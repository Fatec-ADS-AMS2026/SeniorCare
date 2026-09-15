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
        _factory.NotificationSender.Reset(NotificationDeliveryStatus.Disabled);
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

    [Theory]
    [InlineData(NotificationDeliveryStatus.Sent, true)]
    [InlineData(NotificationDeliveryStatus.Failed, false)]
    public async Task Post_ReportsNotificationOutcomeWithoutRollingBackAccount(
        NotificationDeliveryStatus senderStatus,
        bool expectedEmailSent)
    {
        _factory.NotificationSender.Reset(senderStatus);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var email = $"notificacao-{Guid.NewGuid():N}@example.com";

        var response = await _factory.CreateClient().AsUser(adminId).PostAsJsonAsync(
            "/api/v1/AdminUser",
            new AdminUserCreateRequest { Email = email, DisplayName = "Pessoa Notificada" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<AdminUserCreateResponse>();
        created!.EmailSent.Should().Be(expectedEmailSent);
        (await db.Users.AnyAsync(u => u.Email == email)).Should().BeTrue();
        _factory.NotificationSender.Messages.Should().ContainSingle();
    }

    [Theory]
    [InlineData(NotificationDeliveryStatus.Sent, true)]
    [InlineData(NotificationDeliveryStatus.Failed, false)]
    public async Task ResendActivation_ReplacesPendingTokenWithoutExposingIt(
        NotificationDeliveryStatus senderStatus,
        bool expectedEmailSent)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);
        var email = $"reenvio-{Guid.NewGuid():N}@example.com";

        var createResponse = await client.PostAsJsonAsync("/api/v1/AdminUser",
            new AdminUserCreateRequest { Email = email, DisplayName = "Pessoa Reenvio" });
        var created = await createResponse.Content.ReadFromJsonAsync<AdminUserCreateResponse>();
        var originalToken = await db.AccountTokens.SingleAsync(t =>
            t.UserId == created!.Id && t.Purpose == AccountTokenPurpose.ACTIVATION);

        _factory.NotificationSender.Reset(senderStatus);
        var response = await client.PostAsync(
            $"/api/v1/AdminUser/{created!.Id}/resend-activation", null);
        var rawResponse = await response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadFromJsonAsync<ActivationResendResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.EmailSent.Should().Be(expectedEmailSent);
        rawResponse.ToLowerInvariant().Should().NotContain("token");
        db.ChangeTracker.Clear();
        var tokens = await db.AccountTokens
            .Where(t => t.UserId == created.Id && t.Purpose == AccountTokenPurpose.ACTIVATION)
            .OrderBy(t => t.CreatedAtUtc)
            .ToListAsync();
        tokens.Should().HaveCount(2);
        tokens.Single(t => t.Id == originalToken.Id).UsedAtUtc.Should().NotBeNull();
        tokens.Single(t => t.Id != originalToken.Id).UsedAtUtc.Should().BeNull();
        _factory.NotificationSender.Messages.Should().ContainSingle();
    }

    [Fact]
    public async Task ResendActivation_ForAccountFromAnotherInstitution_ReturnsNotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, firstAdminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var (_, secondAdminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var otherClient = _factory.CreateClient().AsUser(secondAdminId);
        var createResponse = await otherClient.PostAsJsonAsync("/api/v1/AdminUser",
            new AdminUserCreateRequest
            {
                Email = $"outra-{Guid.NewGuid():N}@example.com",
                DisplayName = "Outra Instituição"
            });
        var created = await createResponse.Content.ReadFromJsonAsync<AdminUserCreateResponse>();

        var response = await _factory.CreateClient().AsUser(firstAdminId).PostAsync(
            $"/api/v1/AdminUser/{created!.Id}/resend-activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResendActivation_ForActiveAccount_IsRejectedWithoutIssuingToken()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);
        var createResponse = await client.PostAsJsonAsync("/api/v1/AdminUser",
            new AdminUserCreateRequest
            {
                Email = $"ativa-{Guid.NewGuid():N}@example.com",
                DisplayName = "Pessoa Ativa"
            });
        var created = await createResponse.Content.ReadFromJsonAsync<AdminUserCreateResponse>();
        var user = await db.Users.SingleAsync(u => u.Id == created!.Id);
        user.AccountState = AccountState.ACTIVE;
        await db.SaveChangesAsync();
        var tokenCount = await db.AccountTokens.CountAsync(t => t.UserId == user.Id);

        var response = await client.PostAsync(
            $"/api/v1/AdminUser/{user.Id}/resend-activation", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await db.AccountTokens.CountAsync(t => t.UserId == user.Id)).Should().Be(tokenCount);
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
