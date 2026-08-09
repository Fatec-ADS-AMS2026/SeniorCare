using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class SessionRotationTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public SessionRotationTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthenticatedRequest_AfterAccessWindowElapsed_RotatesCookie()
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var originalCookie = ExtractCookie(loginResponse);

        await FastForwardRotationDueAsync(userId);

        var response = await client.GetAsync("/api/v1/Auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotatedCookie = ExtractCookie(response);
        rotatedCookie.Should().NotBeNullOrEmpty();
        rotatedCookie.Should().NotBe(originalCookie);
    }

    [Fact]
    public async Task ReusingRotatedAwayCookie_IsRejectedAndRevokesWholeSession()
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        var oldCookie = ExtractCookie(loginResponse);

        await FastForwardRotationDueAsync(userId);
        var rotateResponse = await client.GetAsync("/api/v1/Auth/me");
        rotateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var replayClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
        replayClient.DefaultRequestHeaders.Add("Cookie", oldCookie);
        var reuseResponse = await replayClient.GetAsync("/api/v1/Auth/me");
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.UserSessions.SingleAsync(s => s.UserId == userId);
        session.RevokedAtUtc.Should().NotBeNull();

        // A sessão inteira foi revogada — nem o cookie rotacionado (o "bom") funciona mais.
        var afterRevocation = await client.GetAsync("/api/v1/Auth/me");
        afterRevocation.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_RevokesSessionAndClearsCookie()
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        var logoutResponse = await client.PostAsync("/api/v1/Auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.UserSessions.SingleAsync(s => s.UserId == userId);
        session.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangePassword_RevokesAllSessionsOfUser()
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        const string newPassword = "Outra-Senha-Bem-Longa-Para-Teste-2026"; // gitleaks:allow — senha fixa de teste, não é segredo
        await client.PostAsJsonAsync("/api/v1/Auth/change-password",
            new ChangePasswordRequest { Email = email, CurrentPassword = TestIdentitySeeder.DefaultTestPassword, NewPassword = newPassword });

        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sessions = await db.UserSessions.Where(s => s.UserId == userId).ToListAsync();
        sessions.Should().OnlyContain(s => s.RevokedAtUtc != null);
    }

    // Deliberadamente "sem grants" (não "acesso total") — acesso total inclui a permissão
    // marcadora AccessAdministration/manage, que por si só torna MFA obrigatório (§7.6) e
    // quebraria a premissa destes testes de sessão/cookie.
    private async Task<(string Email, Guid UserId)> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, userId) = await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;
        return (email, userId);
    }

    // Rotação só acontece depois da janela de acesso curto (default 15 min) — em vez de
    // esperar de verdade, adianta LastRotatedAtUtc no banco, mesma técnica já usada para
    // testar expiração de token na §4.
    private async Task FastForwardRotationDueAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.UserSessions.SingleAsync(s => s.UserId == userId);
        session.LastRotatedAtUtc = DateTime.UtcNow.AddMinutes(-30);
        await db.SaveChangesAsync();
    }

    private static string ExtractCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.FirstOrDefault() : null;
        setCookie.Should().NotBeNullOrEmpty();
        return setCookie!.Split(';')[0];
    }
}
