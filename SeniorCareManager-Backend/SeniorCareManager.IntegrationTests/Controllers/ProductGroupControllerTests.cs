using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

/// <summary>
/// Testes de caracterização do CRUD de ProductGroup.
/// </summary>
public sealed class ProductGroupControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductGroupControllerTests(PostgresWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/v1/ProductGroup");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<ProductGroup>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_ValidProductGroup_ReturnsOkWithCreatedEntity()
    {
        var group = new ProductGroup { Name = "Medicamentos" };

        var response = await _client.PostAsJsonAsync("/api/v1/ProductGroup", group);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ProductGroup>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Medicamentos");
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroup { Name = "Higiene" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroup>();

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
            new ProductGroup { Name = "Alimentos" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroup>();

        var updated = new ProductGroup { Id = created!.Id, Name = "Alimentos Especiais" };
        var response = await _client.PutAsJsonAsync($"/api/v1/ProductGroup/{created.Id}", updated);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsOk()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/ProductGroup",
            new ProductGroup { Name = "Para Deletar" });
        var created = await post.Content.ReadFromJsonAsync<ProductGroup>();

        var response = await _client.DeleteAsync($"/api/v1/ProductGroup/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
