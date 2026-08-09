using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AdminPermissionGroupControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminPermissionGroupControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WithoutPermission_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db);

        var response = await _factory.CreateClient().AsUser(userId).GetAsync("/api/v1/AdminPermissionGroup");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Put_WithStaleRowVersion_ReturnsConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(userId);

        var post = await client.PostAsJsonAsync("/api/v1/AdminPermissionGroup",
            new PermissionGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var created = await post.Content.ReadFromJsonAsync<PermissionGroupDTO>();

        await client.PutAsJsonAsync($"/api/v1/AdminPermissionGroup/{created!.Id}",
            new PermissionGroupUpdateRequest { Name = "Atualizado", RowVersion = created.RowVersion });

        var response = await client.PutAsJsonAsync($"/api/v1/AdminPermissionGroup/{created.Id}",
            new PermissionGroupUpdateRequest { Name = "Edição Concorrente", RowVersion = created.RowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AttachPermission_UnknownId_ReturnsUnprocessableEntity()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(userId);

        var post = await client.PostAsJsonAsync("/api/v1/AdminPermissionGroup",
            new PermissionGroupCreateRequest { Name = $"Grupo {Guid.NewGuid():N}" });
        var created = await post.Content.ReadFromJsonAsync<PermissionGroupDTO>();

        var response = await client.PostAsJsonAsync($"/api/v1/AdminPermissionGroup/{created!.Id}/permissions",
            new PermissionLinkRequest { PermissionId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
