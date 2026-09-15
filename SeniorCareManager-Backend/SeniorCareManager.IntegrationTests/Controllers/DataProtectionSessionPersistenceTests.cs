using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class DataProtectionSessionPersistenceTests : IAsyncLifetime
{
    private const string Password = "Correto-Cavalo-Grampo-2026-Unico"; // gitleaks:allow — senha sintética de teste
    private readonly string _keyRingPath = Path.Combine(Path.GetTempPath(), $"seniorcare-keyring-{Guid.NewGuid():N}");
    private readonly PostgresWebApplicationFactory _factory;

    public DataProtectionSessionPersistenceTests()
    {
        _factory = new PostgresWebApplicationFactory
        {
            DataProtectionKeyRingPath = _keyRingPath,
            DataProtectionApplicationName = "seniorcare-academico",
        };
    }

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_keyRingPath);
        await _factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        Directory.Delete(_keyRingPath, recursive: true);
    }

    [Fact]
    public async Task AuthenticatedSession_RemainsValidAfterApiHostRecreationWithSameKeyRing()
    {
        var email = await CreateActiveUserAsync();
        using var firstClient = _factory.CreateAuthenticatedFlowClient();
        var login = await firstClient.PostAsJsonAsync("/api/v1/Auth/login", new LoginRequest
        {
            Email = email,
            Password = Password,
        });
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookie = login.Headers.GetValues("Set-Cookie").Single().Split(';')[0];

        // WithWebHostBuilder cria um novo host TestServer sobre o mesmo banco do factory;
        // o key ring persistente é carregado novamente do diretório compartilhado.
        using var recreatedHost = _factory.WithWebHostBuilder(_ => { });
        using var recreatedClient = recreatedHost.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        recreatedClient.DefaultRequestHeaders.Add("Cookie", cookie);

        var me = await recreatedClient.GetAsync("/api/v1/Auth/me");

        me.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<string> CreateActiveUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"session-{Guid.NewGuid():N}@seniorcare.example.test";
        var institution = new Institution(Guid.NewGuid(), "Instituição Didática");
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = email, Email = email, EmailConfirmed = true,
            InstitutionId = institution.Id, DisplayName = "Usuário Sintético",
            IdentityOrigin = IdentityOrigin.LOCAL, AccountState = AccountState.ACTIVE,
        };
        (await userManager.CreateAsync(user, Password)).Succeeded.Should().BeTrue();
        return email;
    }
}
