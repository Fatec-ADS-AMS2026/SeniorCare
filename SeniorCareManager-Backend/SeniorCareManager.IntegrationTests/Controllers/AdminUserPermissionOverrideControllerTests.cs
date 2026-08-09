using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminUserPermissionOverrideControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminUserPermissionOverrideControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_PermanentWithoutJustification_ReturnsUnprocessableEntity()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PostAsJsonAsync("/api/v1/AdminUserPermissionOverride", new UserPermissionOverrideCreateRequest
        {
            UserId = targetUserId,
            Resource = "ProductGroup",
            Action = "read",
            Effect = AccessEffect.ALLOW,
            Justification = null,
            ValidFrom = DateTime.UtcNow,
            ValidTo = null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_ThenTargetGetsAccess_RevokeThenDenies()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        var adminClient = _factory.CreateClient().AsUser(adminId);
        var targetClient = _factory.CreateClient().AsUser(targetUserId);

        (await targetClient.GetAsync("/api/v1/Religion")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var createResponse = await adminClient.PostAsJsonAsync("/api/v1/AdminUserPermissionOverride", new UserPermissionOverrideCreateRequest
        {
            UserId = targetUserId,
            Resource = "Religion",
            Action = "read",
            Effect = AccessEffect.ALLOW,
            Justification = "Necessidade temporária documentada em chamado de suporte.",
            ValidFrom = DateTime.UtcNow.AddMinutes(-1),
            ValidTo = DateTime.UtcNow.AddDays(1),
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UserPermissionOverrideDTO>();

        (await targetClient.GetAsync("/api/v1/Religion")).StatusCode.Should().Be(HttpStatusCode.OK);

        await adminClient.PutAsync($"/api/v1/AdminUserPermissionOverride/{created!.Id}/revoke", null);

        (await targetClient.GetAsync("/api/v1/Religion")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
