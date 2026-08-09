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

public sealed class AdminOrganizationalRoleControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminOrganizationalRoleControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_WithoutPermission_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db);

        var response = await _factory.CreateClient().AsUser(userId).PostAsJsonAsync(
            "/api/v1/AdminOrganizationalRole", new OrganizationalRoleCreateRequest { Name = "Coordenação" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_ThenAttachPermissionGroup_And_AssignmentGrantsScopedAccess()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        var client = _factory.CreateClient().AsUser(adminId);

        var rolePost = await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRole",
            new OrganizationalRoleCreateRequest { Name = $"Responsabilidade {Guid.NewGuid():N}" });
        var orgRole = await rolePost.Content.ReadFromJsonAsync<OrganizationalRoleDTO>();

        var groupPost = await client.PostAsJsonAsync("/api/v1/AdminPermissionGroup",
            new SeniorCareManager.WebAPI.Objects.Dtos.Requests.PermissionGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var group = await groupPost.Content.ReadFromJsonAsync<PermissionGroupDTO>();

        var permissionsResponse = await client.GetAsync("/api/v1/AdminPermission?pageSize=100");
        var permissionsBody = await permissionsResponse.Content.ReadFromJsonAsync<
            SeniorCareManager.WebAPI.Objects.Dtos.Common.PagedResult<PermissionDTO>>();
        var readReligionPermission = permissionsBody!.Items.Single(p => p.Resource == "Religion" && p.Action == "read");

        await client.PostAsJsonAsync($"/api/v1/AdminPermissionGroup/{group!.Id}/permissions",
            new PermissionLinkRequest { PermissionId = readReligionPermission.Id });
        await client.PostAsJsonAsync($"/api/v1/AdminOrganizationalRole/{orgRole!.Id}/permission-groups",
            new PermissionGroupLinkRequest { PermissionGroupId = group.Id });

        await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRoleAssignment", new OrganizationalRoleAssignmentCreateRequest
        {
            UserId = targetUserId,
            OrganizationalRoleId = orgRole.Id,
            ScopeType = AccessScopeType.INSTITUTION,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
        });

        var targetClient = _factory.CreateClient().AsUser(targetUserId);
        var response = await targetClient.GetAsync("/api/v1/Religion");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
