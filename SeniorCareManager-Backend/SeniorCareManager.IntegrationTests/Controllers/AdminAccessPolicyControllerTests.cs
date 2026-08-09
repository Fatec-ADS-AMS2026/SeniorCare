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

public sealed class AdminAccessPolicyControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminAccessPolicyControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_CreatesDraftVersion1()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);

        var response = await client.PostAsJsonAsync("/api/v1/AdminAccessPolicy",
            new AccessPolicyCreateRequest { Resource = "ProductGroup", Action = "delete", Effect = AccessEffect.DENY });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<AccessPolicyDTO>();
        created!.Version.Should().Be(1);
        created.State.Should().Be(AccessPolicyState.DRAFT);
    }

    [Fact]
    public async Task Activate_ThenRevise_RetiresPreviousVersion()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var client = _factory.CreateClient().AsUser(adminId);

        var createResponse = await client.PostAsJsonAsync("/api/v1/AdminAccessPolicy",
            new AccessPolicyCreateRequest { Resource = "Religion", Action = "delete", Effect = AccessEffect.DENY });
        var v1 = await createResponse.Content.ReadFromJsonAsync<AccessPolicyDTO>();

        var activateResponse = await client.PutAsync($"/api/v1/AdminAccessPolicy/{v1!.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var reviseResponse = await client.PostAsJsonAsync($"/api/v1/AdminAccessPolicy/{v1.Id}/revise",
            new AccessPolicyCreateRequest { Resource = "Religion", Action = "delete", Effect = AccessEffect.ALLOW });
        var v2 = await reviseResponse.Content.ReadFromJsonAsync<AccessPolicyDTO>();
        v2!.PolicyKey.Should().Be(v1.PolicyKey);
        v2.Version.Should().Be(2);

        await client.PutAsync($"/api/v1/AdminAccessPolicy/{v2.Id}/activate", null);

        var v1AfterResponse = await client.GetAsync($"/api/v1/AdminAccessPolicy/{v1.Id}");
        var v1After = await v1AfterResponse.Content.ReadFromJsonAsync<AccessPolicyDTO>();
        v1After!.State.Should().Be(AccessPolicyState.RETIRED);
    }
}
