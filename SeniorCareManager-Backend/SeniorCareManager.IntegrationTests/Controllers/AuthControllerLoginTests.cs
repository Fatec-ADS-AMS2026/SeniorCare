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

public sealed class AuthControllerLoginTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuthControllerLoginTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ValidCredentialsNoMfa_ReturnsOkAndSetsCookie()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        // "Sem grants", não "acesso total" — acesso total inclui a permissão marcadora
        // AccessAdministration/manage, que torna MFA obrigatório (§7.6) e contradiria o nome
        // deste teste.
        var (_, userId) = await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;

        var client = _factory.CreateAuthenticatedFlowClient();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Status.Should().Be("ok");
        body.Identity.Should().NotBeNull();

        // O cookie do client já foi atualizado automaticamente — /me deve funcionar.
        var me = await client.GetAsync("/api/v1/Auth/me");
        me.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = "senha-completamente-errada" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsSameGenericUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = $"inexistente-{Guid.NewGuid():N}@example.com", Password = "qualquer-coisa-aqui-123" }); // gitleaks:allow — senha fixa de teste, não é segredo

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ProvisionedAccount_ReturnsUnauthorized()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);
        var email = $"provisionado-{Guid.NewGuid():N}@example.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Pendente",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.PROVISIONED,
        };
        await db.SaveChangesAsync();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await userManager.AddPasswordAsync(user, TestIdentitySeeder.DefaultTestPassword);

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
