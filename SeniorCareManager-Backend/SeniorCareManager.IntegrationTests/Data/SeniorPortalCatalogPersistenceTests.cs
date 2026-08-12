using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Data;

// introduce-senior-portal §2.6 — prova as constraints de banco declaradas na migração
// AddSeniorPortalCatalog: unicidade de ModuleDefinition.Key e do par
// {InstitutionId, ModuleDefinitionId}, FK restrict de ModuleDefinitionId, o CHECK
// constraint do range de OperationalState, e a concorrência otimista (xmin) de
// InstitutionModule — mesmas garantias que os demais catálogos já têm, aqui direto
// contra o DbContext (não existe controller pra estas entidades ainda, é trabalho de §3).
public sealed class SeniorPortalCatalogPersistenceTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public SeniorPortalCatalogPersistenceTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(Guid InstitutionId, int ModuleDefinitionId)> SeedInstitutionAndCareModuleAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var institution = new Institution(Guid.NewGuid(), $"ILPI Persistence {Guid.NewGuid()}");
        db.Institutions.Add(institution);
        await db.SaveChangesAsync();
        var careModuleId = await db.ModuleDefinitions.Where(md => md.Key == "care").Select(md => md.Id).SingleAsync();
        return (institution.Id, careModuleId);
    }

    [Fact]
    public async Task ModuleDefinition_DuplicateKey_ThrowsUniqueViolation()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var carePermissionId = await db.ModuleDefinitions.Where(md => md.Key == "care").Select(md => md.RequiredPermissionId).SingleAsync();

        db.ModuleDefinitions.Add(new ModuleDefinition(
            999, "care", "Duplicado", "Descrição", "HeartStraight", "/care", carePermissionId));

        var act = async () => await db.SaveChangesAsync();

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task InstitutionModule_DuplicatePair_ThrowsUniqueViolation()
    {
        var (institutionId, moduleDefinitionId) = await SeedInstitutionAndCareModuleAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        db.InstitutionModules.AddRange(
            new InstitutionModule { InstitutionId = institutionId, ModuleDefinitionId = moduleDefinitionId, Order = 1, CreatedAtUtc = now, UpdatedAtUtc = now },
            new InstitutionModule { InstitutionId = institutionId, ModuleDefinitionId = moduleDefinitionId, Order = 2, CreatedAtUtc = now, UpdatedAtUtc = now });

        var act = async () => await db.SaveChangesAsync();

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task InstitutionModule_UnknownModuleDefinition_ThrowsForeignKeyViolation()
    {
        var (institutionId, _) = await SeedInstitutionAndCareModuleAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        db.InstitutionModules.Add(new InstitutionModule
        {
            InstitutionId = institutionId, ModuleDefinitionId = -1, Order = 1, CreatedAtUtc = now, UpdatedAtUtc = now,
        });

        var act = async () => await db.SaveChangesAsync();

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task InstitutionModule_OperationalStateOutOfRange_ThrowsCheckViolation()
    {
        var (institutionId, moduleDefinitionId) = await SeedInstitutionAndCareModuleAsync();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;
        db.InstitutionModules.Add(new InstitutionModule
        {
            InstitutionId = institutionId,
            ModuleDefinitionId = moduleDefinitionId,
            OperationalState = (OperationalState)99,
            Order = 1,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        });

        var act = async () => await db.SaveChangesAsync();

        var exception = await act.Should().ThrowAsync<DbUpdateException>();
        exception.Which.InnerException.Should().BeOfType<PostgresException>()
            .Which.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task InstitutionModule_ConcurrentUpdate_ThrowsConcurrencyException()
    {
        var (institutionId, moduleDefinitionId) = await SeedInstitutionAndCareModuleAsync();

        using (var seedScope = _factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            db.InstitutionModules.Add(new InstitutionModule
            {
                InstitutionId = institutionId, ModuleDefinitionId = moduleDefinitionId, Order = 1, CreatedAtUtc = now, UpdatedAtUtc = now,
            });
            await db.SaveChangesAsync();
        }

        using var firstScope = _factory.Services.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var firstRead = await firstDb.InstitutionModules.SingleAsync(im => im.InstitutionId == institutionId);

        using var secondScope = _factory.Services.CreateScope();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var secondRead = await secondDb.InstitutionModules.SingleAsync(im => im.InstitutionId == institutionId);

        firstRead.IsEnabled = true;
        await firstDb.SaveChangesAsync();

        secondRead.IsEnabled = true;
        var act = async () => await secondDb.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}
