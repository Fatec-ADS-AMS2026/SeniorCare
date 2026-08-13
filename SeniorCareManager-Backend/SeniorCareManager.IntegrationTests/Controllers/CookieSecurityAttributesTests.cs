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
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

// §8.3 — valida sob TLS os atributos do cookie de sessão (Secure/HttpOnly/SameSite=Strict,
// única defesa CSRF do backend — decisão explícita de não introduzir middleware de token CSRF,
// ver Startup.cs e docs/architecture/senior-portal-contracts.md) e que logout/CORS funcionam
// da mesma forma independente de qual dos três front-ends (care/stock/portal) originou a
// requisição — todos batem no mesmo cookie de sessão compartilhado.
public sealed class CookieSecurityAttributesTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public CookieSecurityAttributesTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_SetsCookie_WithSecureHttpOnlyStrictAttributes()
    {
        var (email, _) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        var setCookie = ExtractRawSetCookie(response);
        setCookie.Should().ContainEquivalentOf("; secure");
        setCookie.Should().ContainEquivalentOf("; httponly");
        setCookie.Should().ContainEquivalentOf("samesite=strict");
    }

    [Fact]
    public async Task RotatedCookie_KeepsSecureHttpOnlyStrictAttributes()
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        await FastForwardRotationDueAsync(userId);
        var response = await client.GetAsync("/api/v1/Auth/me");

        var setCookie = ExtractRawSetCookie(response);
        setCookie.Should().ContainEquivalentOf("; secure");
        setCookie.Should().ContainEquivalentOf("; httponly");
        setCookie.Should().ContainEquivalentOf("samesite=strict");
    }

    [Fact]
    public async Task Logout_ClearsCookie_WithPastExpiry()
    {
        var (email, _) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        var logoutResponse = await client.PostAsync("/api/v1/Auth/logout", null);

        var setCookie = ExtractRawSetCookie(logoutResponse);
        // ASP.NET Core limpa o cookie reenviando-o vazio com data no passado — o navegador
        // descarta ao processar essa resposta, em qualquer aba/app aberta na mesma origem.
        setCookie.Should().ContainEquivalentOf("; secure");
        setCookie.Should().Contain("expires=");
        setCookie.Should().Contain("1970"); // data no passado — o navegador descarta o cookie ao processar
    }

    // Simula requisições originadas de cada um dos três front-ends (Origin distinto por
    // requisição) reutilizando o mesmo cookie de sessão — comprova que login/logout não têm
    // afinidade com o app que os originou, condizente com a sessão única compartilhada (§7.4,
    // §8.3: "logout iniciado em qualquer aplicação").
    [Theory]
    [InlineData("https://care.exemplo.com.br")]
    [InlineData("https://estoque.exemplo.com.br")]
    [InlineData("https://portal.exemplo.com.br")]
    public async Task Logout_FromAnyAllowedOrigin_RevokesSharedSession(string origin)
    {
        var (email, userId) = await CreateActiveUserAsync();
        var client = _factory.CreateAuthenticatedFlowClient();
        await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        using var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Auth/logout");
        logoutRequest.Headers.Add("Origin", origin);
        var logoutResponse = await client.SendAsync(logoutRequest);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync("/api/v1/Auth/me")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.UserSessions.SingleAsync(s => s.UserId == userId);
        session.RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PreflightFromAllowedOrigin_IsAccepted()
    {
        var client = _factory.CreateAuthenticatedFlowClient();
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/v1/Auth/login");
        preflight.Headers.Add("Origin", "http://localhost:3002");
        preflight.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(preflight);

        response.Headers.TryGetValues("Access-Control-Allow-Origin", out var allowed).Should().BeTrue();
        allowed!.Single().Should().Be("http://localhost:3002");
    }

    [Fact]
    public async Task PreflightFromDisallowedOrigin_GetsNoCorsHeaders()
    {
        var client = _factory.CreateAuthenticatedFlowClient();
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/api/v1/Auth/login");
        preflight.Headers.Add("Origin", "https://attacker.example");
        preflight.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(preflight);

        // Sem Access-Control-Allow-Origin, o navegador da vítima descarta a resposta — é essa
        // ausência (não um status de erro) que bloqueia a origem não listada.
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }

    private async Task<(string Email, Guid UserId)> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, userId) = await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;
        return (email, userId);
    }

    private async Task FastForwardRotationDueAsync(Guid userId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await db.UserSessions.SingleAsync(s => s.UserId == userId);
        session.LastRotatedAtUtc = DateTime.UtcNow.AddMinutes(-30);
        await db.SaveChangesAsync();
    }

    private static string ExtractRawSetCookie(HttpResponseMessage response)
    {
        var setCookie = response.Headers.TryGetValues("Set-Cookie", out var values) ? values.FirstOrDefault() : null;
        setCookie.Should().NotBeNullOrEmpty();
        return setCookie!;
    }
}
