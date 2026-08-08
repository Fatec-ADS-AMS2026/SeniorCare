using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Services;

public sealed class BootstrapServiceTests : IClassFixture<BootstrapPostgresWebApplicationFactory>
{
    private readonly BootstrapPostgresWebApplicationFactory _factory;

    public BootstrapServiceTests(BootstrapPostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task RunAsync_CalledTwice_CreatesInstitutionAndAdminOnlyOnce()
    {
        using var firstScope = _factory.Services.CreateScope();
        var firstResult = await firstScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        using var secondScope = _factory.Services.CreateScope();
        var secondResult = await secondScope.ServiceProvider.GetRequiredService<IBootstrapService>().RunAsync();

        firstResult.Created.Should().BeTrue();
        firstResult.AdminEmail.Should().Be(BootstrapPostgresWebApplicationFactory.AdminEmail);
        firstResult.ActivationToken.Should().NotBeNullOrWhiteSpace();

        // Idempotente: a segunda chamada não recria nem redefine nada.
        secondResult.Created.Should().BeFalse();
        secondResult.AdminEmail.Should().BeNull();

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Institutions.CountAsync()).Should().Be(1);
        (await db.Users.CountAsync()).Should().Be(1);
    }
}
