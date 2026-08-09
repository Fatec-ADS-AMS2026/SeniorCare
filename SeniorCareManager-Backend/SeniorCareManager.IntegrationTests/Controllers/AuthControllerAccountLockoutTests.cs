using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.IntegrationTests.Controllers;

// Classe própria: o limitador por origem (§7.8) é singleton por fábrica/processo — isolar
// numa fábrica dedicada evita que as tentativas daqui contaminem outros testes de login.
public sealed class AuthControllerAccountLockoutTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuthControllerAccountLockoutTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ExceedsFailedAttempts_LocksAccountEvenWithCorrectPasswordAfterward()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;

        var client = _factory.CreateClient();
        for (var i = 0; i < InstitutionSecurityPolicyService.DefaultMaxFailedAttempts; i++)
        {
            var attempt = await client.PostAsJsonAsync("/api/v1/Auth/login",
                new LoginRequest { Email = email, Password = "senha-errada-de-propósito" });
            attempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // A conta já está bloqueada — nem a senha correta funciona até o bloqueio expirar.
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
