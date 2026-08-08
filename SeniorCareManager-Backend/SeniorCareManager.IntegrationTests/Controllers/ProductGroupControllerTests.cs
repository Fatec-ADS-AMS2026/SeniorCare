using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;

namespace SeniorCareManager.IntegrationTests.Controllers;

/// <summary>
/// Testes de caracterização do CRUD de ProductGroup. Autentica com um usuário de acesso
/// total (§5) — o alvo destes testes é o comportamento do CRUD, não autorização.
/// </summary>
public sealed class ProductGroupControllerTests : IClassFixture<PostgresWebApplicationFactory>, IAsyncLifetime
{
    private readonly PostgresWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public ProductGroupControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        _client.AsUser(userId);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_ReturnsOkWithPagedResult()
    {
        var response = await _client.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedResult<ProductGroupDTO>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_ValidProductGroup_ReturnsCreatedWithEntity()
    {
        var request = new ProductGroupCreateRequest { Name = "Medicamentos" };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductGroup", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProductGroupDTO>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Medicamentos");
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = "Higiene" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroupDTO>();

        var response = await _client.GetAsync($"/api/v1/ProductGroup/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/ProductGroup/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ExistingId_ReturnsOk()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = "Alimentos" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroupDTO>();

        var updated = new ProductGroupUpdateRequest { Name = "Alimentos Especiais", RowVersion = created!.RowVersion };
        var response = await _client.PutAsJsonAsync($"/api/v1/ProductGroup/{created.Id}", updated);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_WithStaleRowVersion_ReturnsConflict()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = "Suplementos" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroupDTO>();

        await _client.PutAsJsonAsync($"/api/v1/ProductGroup/{created!.Id}",
            new ProductGroupUpdateRequest { Name = "Suplementos Atualizado", RowVersion = created.RowVersion });

        var response = await _client.PutAsJsonAsync($"/api/v1/ProductGroup/{created.Id}",
            new ProductGroupUpdateRequest { Name = "Edição Concorrente", RowVersion = created.RowVersion });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Patch_NoLongerExists_ReturnsMethodNotAllowed()
    {
        // Tarefa 3.6: PATCH de substituição total foi removido — a rota existe (GET/
        // PUT/DELETE), mas não tem handler PATCH, logo 405 (não 404).
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = "Higiene Bucal" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroupDTO>();

        var response = await _client.PatchAsync(
            $"/api/v1/ProductGroup/{created!.Id}",
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroupCreateRequest { Name = "Para Deletar" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroupDTO>();

        var response = await _client.DeleteAsync($"/api/v1/ProductGroup/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
