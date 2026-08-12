using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Controllers;

// introduce-senior-portal §3.2/§3.7 — GET /api/v1/me/modules.
public sealed class ModuleCatalogControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public ModuleCatalogControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task ProvisionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>().RunAsync();
    }

    private async Task SetModuleStateAsync(
        Guid institutionId, string moduleKey, bool isEnabled, OperationalState state, string? message = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var institutionModule = await db.InstitutionModules
            .Include(im => im.ModuleDefinition)
            .SingleAsync(im => im.InstitutionId == institutionId && im.ModuleDefinition!.Key == moduleKey);
        institutionModule.IsEnabled = isEnabled;
        institutionModule.OperationalState = state;
        institutionModule.OperationalMessage = message;
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_EnabledAvailableModuleWithPermission_ReturnsIt()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionId, "care", isEnabled: true, OperationalState.AVAILABLE);

        var client = _factory.CreateClient().AsUser(userId);
        var response = await client.GetAsync("/api/v1/me/modules");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var modules = await response.Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();
        modules.Should().ContainSingle(m => m.Key == "care" && m.OperationalState == OperationalState.AVAILABLE);
    }

    [Fact]
    public async Task Get_ModuleWithoutPermission_IsOmitted()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, _) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var noGrantsUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionId, "care", isEnabled: true, OperationalState.AVAILABLE);

        var client = _factory.CreateClient().AsUser(noGrantsUserId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ModuleNeverEnabled_IsOmitted()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        // Estado padrão do provisionamento (§2.3): DISABLED/IsEnabled=false — nunca tocado.

        var client = _factory.CreateClient().AsUser(userId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_MaintenanceModule_ReturnsWithOperationalMessage()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionId, "stock", isEnabled: true, OperationalState.MAINTENANCE, "Em manutenção programada.");

        var client = _factory.CreateClient().AsUser(userId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().ContainSingle(m =>
            m.Key == "stock" && m.OperationalState == OperationalState.MAINTENANCE && m.OperationalMessage == "Em manutenção programada.");
    }

    [Fact]
    public async Task Get_TwoInstitutions_DoesNotLeakAcrossInstitutions()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionAId, userAId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var (institutionBId, _) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionAId, "care", isEnabled: true, OperationalState.AVAILABLE);
        await SetModuleStateAsync(institutionBId, "care", isEnabled: true, OperationalState.AVAILABLE);
        await SetModuleStateAsync(institutionBId, "stock", isEnabled: true, OperationalState.AVAILABLE);

        var client = _factory.CreateClient().AsUser(userAId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().ContainSingle(m => m.Key == "care");
    }

    [Fact]
    public async Task Get_OrderingIsDeterministicByOrder()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionId, "care", isEnabled: true, OperationalState.AVAILABLE);
        await SetModuleStateAsync(institutionId, "stock", isEnabled: true, OperationalState.AVAILABLE);

        using (var reorderScope = _factory.Services.CreateScope())
        {
            var reorderDb = reorderScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var care = await reorderDb.InstitutionModules.Include(im => im.ModuleDefinition)
                .SingleAsync(im => im.InstitutionId == institutionId && im.ModuleDefinition!.Key == "care");
            var stock = await reorderDb.InstitutionModules.Include(im => im.ModuleDefinition)
                .SingleAsync(im => im.InstitutionId == institutionId && im.ModuleDefinition!.Key == "stock");
            // Inverte a ordem natural de provisionamento (care=1, stock=2) pra provar que a
            // resposta segue Order, não a ordem de inserção/Id.
            care.Order = 2;
            stock.Order = 1;
            await reorderDb.SaveChangesAsync();
        }

        var client = _factory.CreateClient().AsUser(userId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().HaveCount(2);
        modules![0].Key.Should().Be("stock");
        modules[1].Key.Should().Be("care");
    }

    [Fact]
    public async Task Get_BlockedAccount_ReturnsEmptyCatalog()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAsync();
        await SetModuleStateAsync(institutionId, "care", isEnabled: true, OperationalState.AVAILABLE);

        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.AccountState = AccountState.BLOCKED;
        await db.SaveChangesAsync();

        var client = _factory.CreateClient().AsUser(userId);
        var modules = await (await client.GetAsync("/api/v1/me/modules")).Content.ReadFromJsonAsync<List<ModuleCatalogItemDTO>>();

        modules.Should().BeEmpty();
    }
}
