using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

// HTTP real (5.10): 401 anônimo, 403 sem grant, 200 com grant efetivo. AuthController
// continua público (sessão só chega na §7).
public sealed class EndpointAuthorizationTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public EndpointAuthorizationTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Anonymous_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_AuthenticatedWithoutGrant_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await CreateActiveUserWithNoGrantsAsync(db);

        var client = _factory.CreateClient().AsUser(userId);
        var response = await client.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_AuthenticatedWithFullAccessRole_ReturnsOk()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);

        var client = _factory.CreateClient().AsUser(userId);
        var response = await client.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Recover_Anonymous_ReturnsOk()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/recover",
            new RecoverAccountRequest { Email = "inexistente@example.com" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> CreateActiveUserWithNoGrantsAsync(AppDbContext db)
    {
        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"sem-grant-{Guid.NewGuid():N}@example.com",
            Email = $"sem-grant-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Usuário Sem Permissão",
            AccountState = AccountState.ACTIVE
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }
}
