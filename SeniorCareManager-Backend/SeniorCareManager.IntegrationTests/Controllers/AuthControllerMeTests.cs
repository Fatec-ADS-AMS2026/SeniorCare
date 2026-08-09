using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AuthControllerMeTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuthControllerMeTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMe_Anonymous_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/Auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_FullAccessUser_ListsRoleAndManyEffectivePermissions()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(userId);

        var response = await client.GetAsync("/api/v1/Auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<CurrentIdentityDTO>();
        me!.InstitutionId.Should().Be(institutionId);
        me.Roles.Should().ContainSingle();
        me.EffectivePermissions.Should().Contain(p => p.Resource == "ProductGroup" && p.Action == "read");
    }
}
