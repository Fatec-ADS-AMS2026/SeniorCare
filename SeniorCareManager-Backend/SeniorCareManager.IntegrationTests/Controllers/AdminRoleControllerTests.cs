using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminRoleControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminRoleControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_WithoutPermission_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db);

        var client = _factory.CreateClient().AsUser(userId);
        var response = await client.PostAsJsonAsync("/api/v1/AdminRole", new RoleCreateRequest { Name = "Enfermagem" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Put_WithStaleRowVersion_ReturnsConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(userId);

        var post = await client.PostAsJsonAsync("/api/v1/AdminRole", new RoleCreateRequest { Name = $"Papel {Guid.NewGuid():N}" });
        var created = await post.Content.ReadFromJsonAsync<RoleDTO>();

        await client.PutAsJsonAsync($"/api/v1/AdminRole/{created!.Id}",
            new RoleUpdateRequest { Name = "Atualizado", RowVersion = created.RowVersion });

        var response = await client.PutAsJsonAsync($"/api/v1/AdminRole/{created.Id}",
            new RoleUpdateRequest { Name = "Edição Concorrente", RowVersion = created.RowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AttachPermissionGroup_ThenAssignToUser_GrantsAccess()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        var adminClient = _factory.CreateClient().AsUser(adminId);

        var groupPost = await adminClient.PostAsJsonAsync("/api/v1/AdminPermissionGroup",
            new PermissionGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var group = await groupPost.Content.ReadFromJsonAsync<PermissionGroupDTO>();

        var permissionsResponse = await adminClient.GetAsync("/api/v1/AdminPermission?pageSize=100");
        var permissionsBody = await permissionsResponse.Content.ReadFromJsonAsync<
            SeniorCareManager.WebAPI.Objects.Dtos.Common.PagedResult<PermissionDTO>>();
        var readProductGroupPermission = permissionsBody!.Items.Single(p => p.Resource == "ProductGroup" && p.Action == "read");

        await adminClient.PostAsJsonAsync($"/api/v1/AdminPermissionGroup/{group!.Id}/permissions",
            new PermissionLinkRequest { PermissionId = readProductGroupPermission.Id });

        var rolePost = await adminClient.PostAsJsonAsync("/api/v1/AdminRole", new RoleCreateRequest { Name = $"Papel {Guid.NewGuid():N}" });
        var role = await rolePost.Content.ReadFromJsonAsync<RoleDTO>();

        await adminClient.PostAsJsonAsync($"/api/v1/AdminRole/{role!.Id}/permission-groups",
            new PermissionGroupLinkRequest { PermissionGroupId = group.Id });
        await adminClient.PostAsJsonAsync($"/api/v1/AdminRole/{role.Id}/users",
            new UserRoleAssignmentRequest { UserId = targetUserId });

        var targetClient = _factory.CreateClient().AsUser(targetUserId);
        var response = await targetClient.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
