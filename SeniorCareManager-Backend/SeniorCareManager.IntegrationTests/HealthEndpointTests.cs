using System.Net;
using FluentAssertions;
using SeniorCareManager.IntegrationTests.Infrastructure;

namespace SeniorCareManager.IntegrationTests;

/// <summary>
/// Cenários "Processo vivo com banco indisponível" e "Serviço pronto" da spec
/// runtime-configuration: /health/live nunca depende do banco; /health/ready reflete
/// a disponibilidade real da dependência.
/// </summary>
public sealed class HealthEndpointReadyTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointReadyTests(PostgresWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_RetornaOk()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_ComBancoDisponivelEMigrado_RetornaOk()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public sealed class HealthEndpointUnavailableTests : IClassFixture<UnavailableDatabaseWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointUnavailableTests(UnavailableDatabaseWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Live_ComBancoIndisponivel_ContinuaOk()
    {
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_ComBancoIndisponivel_RetornaServiceUnavailable()
    {
        var response = await _client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
