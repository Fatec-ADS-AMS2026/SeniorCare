using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Services.Entities;

namespace SeniorCareManager.IntegrationTests.Controllers;

// Classe própria pelo mesmo motivo da de bloqueio por conta — isola o estado do
// IOriginRateLimiter (singleton por fábrica) desta suíte.
public sealed class AuthControllerOriginRateLimitTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuthControllerOriginRateLimitTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ExceedsOriginFailureThreshold_ReturnsTooManyRequests()
    {
        var client = _factory.CreateClient();

        // E-mails inexistentes diferentes a cada tentativa — não aciona bloqueio por conta,
        // só o limite por origem (mesmo IP de loopback em todas as chamadas do teste).
        for (var i = 0; i < OriginRateLimiter.MaxFailuresPerWindow; i++)
        {
            var attempt = await client.PostAsJsonAsync("/api/v1/Auth/login",
                new LoginRequest { Email = $"inexistente-{Guid.NewGuid():N}@example.com", Password = "qualquer-coisa-123" }); // gitleaks:allow — senha fixa de teste, não é segredo
            attempt.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = $"inexistente-{Guid.NewGuid():N}@example.com", Password = "qualquer-coisa-123" }); // gitleaks:allow — senha fixa de teste, não é segredo

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
