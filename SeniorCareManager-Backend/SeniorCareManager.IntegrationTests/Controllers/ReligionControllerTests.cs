using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;

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
        var body = await response.Content.ReadFromJsonAsync<List<ReligionDTO>>();
        body.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_ValidReligion_ReturnsCreatedWithEntity()
    {
        var request = new ReligionCreateRequest { Name = "Católica" };

        var response = await _client.PostAsJsonAsync("/api/v1/Religion", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var body = await response.Content.ReadFromJsonAsync<ReligionDTO>();
        body.Should().NotBeNull();
        body!.Name.Should().Be("Católica");
        body.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Post_EmptyName_ReturnsBadRequest()
    {
        var request = new ReligionCreateRequest { Name = string.Empty };

        var response = await _client.PostAsJsonAsync("/api/v1/Religion", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey(nameof(ReligionCreateRequest.Name));
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOk()
    {
        var created = new ReligionCreateRequest { Name = "Evangélica" };
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", created);
        post.EnsureSuccessStatusCode();
        var body = await post.Content.ReadFromJsonAsync<ReligionDTO>();

        var response = await _client.GetAsync($"/api/v1/Religion/{body!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<ReligionDTO>();
        fetched!.Name.Should().Be("Evangélica");
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFoundAsProblemDetailsWithoutInternalDetails()
    {
        var response = await _client.GetAsync("/api/v1/Religion/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Extensions.Should().ContainKey("correlationId");
        // Nada de stack trace, nome de classe interna ou caminho de arquivo no corpo.
        problem.Detail.Should().NotContain("Exception");
        problem.Detail.Should().NotContain(".cs:");
    }

    [Fact]
    public async Task Put_ExistingId_ReturnsOkWithUpdatedEntity()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", new ReligionCreateRequest { Name = "Espírita" });
        var created = await post.Content.ReadFromJsonAsync<ReligionDTO>();

        var updated = new ReligionUpdateRequest { Name = "Espírita Kardecista" };
        var response = await _client.PutAsJsonAsync($"/api/v1/Religion/{created!.Id}", updated);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ReligionDTO>();
        body!.Name.Should().Be("Espírita Kardecista");
    }

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        var post = await _client.PostAsJsonAsync("/api/v1/Religion", new ReligionCreateRequest { Name = "Para Deletar" });
        var created = await post.Content.ReadFromJsonAsync<ReligionDTO>();

        var response = await _client.DeleteAsync($"/api/v1/Religion/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/v1/Religion/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
