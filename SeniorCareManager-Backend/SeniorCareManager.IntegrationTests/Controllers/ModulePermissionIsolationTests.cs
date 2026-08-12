using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Controllers;

// introduce-senior-portal §3.6 — a permissão Module/care|stock só governa se o card aparece
// no catálogo (GET /api/v1/me/modules); ela NUNCA substitui a permissão de domínio (ex.:
// Product/read) que protege de fato os endpoints do módulo. spec.md "Descoberta de módulos
// usa permissões efetivas": "cada módulo e API SHALL revalidar a autorização da operação
// solicitada" — não confiar na visibilidade do catálogo como credencial.
public sealed class ModulePermissionIsolationTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public ModulePermissionIsolationTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ModuleVisibilityGrant_DoesNotGrantDomainPermission()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        var adminClient = _factory.CreateClient().AsUser(adminId);
        await adminClient.PostAsJsonAsync("/api/v1/AdminUserPermissionOverride", new UserPermissionOverrideCreateRequest
        {
            UserId = userId,
            Resource = "Module",
            Action = "care",
            Effect = AccessEffect.ALLOW,
            Justification = "Concede só a visibilidade do módulo assistencial para o teste.",
            ValidFrom = DateTime.UtcNow.AddMinutes(-1),
            ValidTo = null,
        });

        // Tem a permissão que faria o card "care" aparecer no catálogo, mas nenhuma
        // permissão sobre um recurso de domínio real (ex.: Product) — o acesso direto ao
        // recurso continua negado.
        var userClient = _factory.CreateClient().AsUser(userId);
        (await userClient.GetAsync("/api/v1/Product")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DomainPermissionGrant_DoesNotGrantModuleVisibility()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var userId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        using (var provisionScope = _factory.Services.CreateScope())
        {
            await provisionScope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>().RunAsync();
        }

        var adminClient = _factory.CreateClient().AsUser(adminId);
        await adminClient.PostAsJsonAsync("/api/v1/AdminUserPermissionOverride", new UserPermissionOverrideCreateRequest
        {
            UserId = userId,
            Resource = "Product",
            Action = "read",
            Effect = AccessEffect.ALLOW,
            Justification = "Concede só o domínio Product para o teste, sem Module/care.",
            ValidFrom = DateTime.UtcNow.AddMinutes(-1),
            ValidTo = null,
        });

        var userClient = _factory.CreateClient().AsUser(userId);
        // Tem acesso de domínio real (Product), mas o catálogo continua vazio porque
        // Module/care nunca foi concedido — a visibilidade não "vaza" de uma permissão de
        // domínio não relacionada.
        (await userClient.GetAsync("/api/v1/Product")).StatusCode.Should().Be(HttpStatusCode.OK);
        var modules = await (await userClient.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();
        modules.Should().BeEmpty();
    }
}
