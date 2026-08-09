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

public sealed class AdminOrganizationalRoleAssignmentControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminOrganizationalRoleAssignmentControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_UserFromAnotherInstitution_ReturnsUnprocessableEntity()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var (_, outsiderUserId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db); // outra instituição

        var client = _factory.CreateClient().AsUser(adminId);
        var rolePost = await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRole",
            new OrganizationalRoleCreateRequest { Name = $"Responsabilidade {Guid.NewGuid():N}" });
        var orgRole = await rolePost.Content.ReadFromJsonAsync<OrganizationalRoleDTO>();

        var response = await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRoleAssignment", new OrganizationalRoleAssignmentCreateRequest
        {
            UserId = outsiderUserId,
            OrganizationalRoleId = orgRole!.Id,
            ScopeType = AccessScopeType.INSTITUTION,
            ValidFrom = DateTime.UtcNow,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task End_SetsValidToNow()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        var client = _factory.CreateClient().AsUser(adminId);

        var rolePost = await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRole",
            new OrganizationalRoleCreateRequest { Name = $"Responsabilidade {Guid.NewGuid():N}" });
        var orgRole = await rolePost.Content.ReadFromJsonAsync<OrganizationalRoleDTO>();

        var createResponse = await client.PostAsJsonAsync("/api/v1/AdminOrganizationalRoleAssignment", new OrganizationalRoleAssignmentCreateRequest
        {
            UserId = targetUserId,
            OrganizationalRoleId = orgRole!.Id,
            ScopeType = AccessScopeType.INSTITUTION,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
        });
        var assignment = await createResponse.Content.ReadFromJsonAsync<OrganizationalRoleAssignmentDTO>();

        var response = await client.PutAsync($"/api/v1/AdminOrganizationalRoleAssignment/{assignment!.Id}/end", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ended = await response.Content.ReadFromJsonAsync<OrganizationalRoleAssignmentDTO>();
        ended!.ValidTo.Should().NotBeNull();
        ended.ValidTo.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(5));
    }
}
