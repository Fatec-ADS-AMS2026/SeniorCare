using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminAccessDecisionControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminAccessDecisionControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Explain_GrantedResource_ReturnsAllowedWithRbacLayer()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PostAsJsonAsync("/api/v1/AdminAccessDecision/explain",
            new AccessDecisionExplainRequest { UserId = adminId, Resource = "ProductGroup", Action = "read" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var decision = await response.Content.ReadFromJsonAsync<AccessDecision>();
        decision!.Allowed.Should().BeTrue();
        decision.DeterminingLayer.Should().Be("RBAC");
    }

    [Fact]
    public async Task Explain_UserFromAnotherInstitution_ReturnsNotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var (_, outsiderUserId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PostAsJsonAsync("/api/v1/AdminAccessDecision/explain",
            new AccessDecisionExplainRequest { UserId = outsiderUserId, Resource = "ProductGroup", Action = "read" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
