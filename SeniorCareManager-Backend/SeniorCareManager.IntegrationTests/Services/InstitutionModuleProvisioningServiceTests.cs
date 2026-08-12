using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Services;

// introduce-senior-portal §2.3/§2.6 — usa PostgresWebApplicationFactory (sem bootstrap), já
// que o objetivo é testar o provisionamento em si contra instituições criadas diretamente
// pelo teste, não o fluxo de bootstrap (coberto por BootstrapServiceTests). Cada teste cria
// sua própria instituição com nome único, então é seguro compartilhar o container entre
// métodos desta classe.
public sealed class InstitutionModuleProvisioningServiceTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public InstitutionModuleProvisioningServiceTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<Guid> CreateInstitutionAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var institution = new Institution(Guid.NewGuid(), name);
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();
        return institution.Id;
    }

    [Fact]
    public async Task RunAsync_NewInstitution_ProvisionsAllActiveModulesAsDisabled()
    {
        var institutionId = await CreateInstitutionAsync($"ILPI Provisioning {Guid.NewGuid()}");

        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>();
        var created = await service.RunAsync();

        created.Should().BeGreaterThanOrEqualTo(2); // pelo menos care + stock, seedados via HasData

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rows = await db.InstitutionModules
            .Where(im => im.InstitutionId == institutionId)
            .Include(im => im.ModuleDefinition)
            .ToListAsync();

        rows.Should().Contain(r => r.ModuleDefinition!.Key == "care");
        rows.Should().Contain(r => r.ModuleDefinition!.Key == "stock");
        rows.Should().OnlyContain(r => r.OperationalState == OperationalState.DISABLED);
        rows.Should().OnlyContain(r => !r.IsEnabled);
        rows.Select(r => r.Order).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public async Task RunAsync_CalledTwice_DoesNotDuplicateRows()
    {
        var institutionId = await CreateInstitutionAsync($"ILPI Idempotencia {Guid.NewGuid()}");

        using (var firstScope = _factory.Services.CreateScope())
        {
            await firstScope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>().RunAsync();
        }

        int secondRunCreated;
        using (var secondScope = _factory.Services.CreateScope())
        {
            secondRunCreated = await secondScope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>().RunAsync();
        }

        secondRunCreated.Should().Be(0);

        using var assertScope = _factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var moduleDefinitionCount = await db.ModuleDefinitions.CountAsync(md => md.IsActive);
        var rowCount = await db.InstitutionModules.CountAsync(im => im.InstitutionId == institutionId);

        rowCount.Should().Be(moduleDefinitionCount);
    }
}
