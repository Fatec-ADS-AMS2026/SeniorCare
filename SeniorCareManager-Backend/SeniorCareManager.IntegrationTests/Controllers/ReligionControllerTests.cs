using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

/// <summary>
/// Testes de caracterização do CRUD de Religion.
/// Capturam o contrato atual dos endpoints antes de qualquer alteração,
/// impedindo regressões silenciosas.
/// </summary>
public sealed class ReligionControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReligionControllerTests(PostgresWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsOkWithList()
    {
        var response = await _client.GetAsync("/api/v1/Religion");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<Religion>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_ValidReligion_ReturnsOkWithCreatedEntity()
    {
        var religion = new Religion { Name = "Católica" };

        var response = await _client.PostAsJsonAsync("/api/v1/Religion", religion);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Religion>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Católica");
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var created = new Religion { Name = "Evangélica" };
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", created);
        post.EnsureSuccessStatusCode();
        var body = await post.Content.ReadFromJsonAsync<Religion>();

        var response = await _client.GetAsync($"/api/v1/Religion/{body!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<Religion>();
        fetched!.Name.Should().Be("Evangélica");
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/Religion/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ExistingId_ReturnsOkWithUpdatedEntity()
    {
        var religion = new Religion { Name = "Espírita" };
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", religion);
        var created = await post.Content.ReadFromJsonAsync<Religion>();

        var updated = new Religion { Id = created!.Id, Name = "Espírita Kardecista" };
        var response = await _client.PutAsJsonAsync($"/api/v1/Religion/{created.Id}", updated);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsOk()
    {
        var religion = new Religion { Name = "Para Deletar" };
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", religion);
        var created = await post.Content.ReadFromJsonAsync<Religion>();

        var response = await _client.DeleteAsync($"/api/v1/Religion/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
