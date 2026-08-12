using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Controllers;

// introduce-senior-portal §3.3/§3.4/§3.5/§3.7 — administração do catálogo institucional.
public sealed class AdminInstitutionModuleControllerTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AdminInstitutionModuleControllerTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<int> ProvisionAndGetCareIdAsync(Guid institutionId)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IInstitutionModuleProvisioningService>().RunAsync();
        }

        using var readScope = _factory.Services.CreateScope();
        var db = readScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var care = await db.InstitutionModules.Include(im => im.ModuleDefinition)
            .SingleAsync(im => im.InstitutionId == institutionId && im.ModuleDefinition!.Key == "care");
        return care.Id;
    }

    [Fact]
    public async Task Get_WithReadPermission_ReturnsInstitutionModules()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAndGetCareIdAsync(institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.GetAsync("/api/v1/AdminInstitutionModule");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var modules = await response.Content.ReadFromJsonAsync<List<InstitutionModuleAdminDTO>>();
        modules.Should().Contain(m => m.ModuleKey == "care" && m.OperationalState == OperationalState.DISABLED && !m.IsEnabled);
        modules.Should().Contain(m => m.ModuleKey == "stock");
    }

    [Fact]
    public async Task Get_WithoutPermission_ReturnsForbidden()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, _) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var noGrantsUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);
        await ProvisionAndGetCareIdAsync(institutionId);

        var client = _factory.CreateClient().AsUser(noGrantsUserId);
        var response = await client.GetAsync("/api/v1/AdminInstitutionModule");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Put_ValidUpdate_ChangesStateAndRecordsAudit()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var careId = await ProvisionAndGetCareIdAsync(institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var current = await (await client.GetAsync($"/api/v1/AdminInstitutionModule/{careId}"))
            .Content.ReadFromJsonAsync<InstitutionModuleAdminDTO>();

        var putResponse = await client.PutAsJsonAsync($"/api/v1/AdminInstitutionModule/{careId}", new InstitutionModuleUpdateRequest
        {
            IsEnabled = true,
            Order = 1,
            OperationalState = OperationalState.MAINTENANCE,
            OperationalMessage = "Em manutenção programada.",
            RowVersion = current!.RowVersion,
        });

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await putResponse.Content.ReadFromJsonAsync<InstitutionModuleAdminDTO>();
        updated!.IsEnabled.Should().BeTrue();
        updated.OperationalState.Should().Be(OperationalState.MAINTENANCE);
        updated.OperationalMessage.Should().Be("Em manutenção programada.");

        using var auditScope = _factory.Services.CreateScope();
        var auditDb = auditScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var auditEvent = await auditDb.AuditEvents
            .Where(a => a.InstitutionId == institutionId && a.Resource == "InstitutionModule" && a.Action == "Update")
            .OrderByDescending(a => a.OccurredAtUtc)
            .FirstOrDefaultAsync();

        auditEvent.Should().NotBeNull();
        auditEvent!.ActorUserId.Should().Be(adminId);
        auditEvent.Outcome.Should().Be(AuditOutcome.SUCCESS);
        auditEvent.AfterValue.Should().Contain("MAINTENANCE");
    }

    [Fact]
    public async Task Put_StaleRowVersion_ReturnsConflict()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var careId = await ProvisionAndGetCareIdAsync(institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var current = await (await client.GetAsync($"/api/v1/AdminInstitutionModule/{careId}"))
            .Content.ReadFromJsonAsync<InstitutionModuleAdminDTO>();

        // Primeira alteração — sucede e avança a versão.
        (await client.PutAsJsonAsync($"/api/v1/AdminInstitutionModule/{careId}", new InstitutionModuleUpdateRequest
        {
            IsEnabled = true,
            Order = 1,
            OperationalState = OperationalState.AVAILABLE,
            RowVersion = current!.RowVersion,
        })).StatusCode.Should().Be(HttpStatusCode.OK);

        // Segunda alteração reusa a MESMA versão já superada pela primeira — deve ser
        // rejeitada, nunca sobrescrever silenciosamente (§3.4).
        var staleResponse = await client.PutAsJsonAsync($"/api/v1/AdminInstitutionModule/{careId}", new InstitutionModuleUpdateRequest
        {
            IsEnabled = false,
            Order = 1,
            OperationalState = OperationalState.UNAVAILABLE,
            RowVersion = current.RowVersion,
        });

        staleResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_OperationalMessageWithClinicalTerm_ReturnsUnprocessableEntity()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var careId = await ProvisionAndGetCareIdAsync(institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var current = await (await client.GetAsync($"/api/v1/AdminInstitutionModule/{careId}"))
            .Content.ReadFromJsonAsync<InstitutionModuleAdminDTO>();

        var response = await client.PutAsJsonAsync($"/api/v1/AdminInstitutionModule/{careId}", new InstitutionModuleUpdateRequest
        {
            IsEnabled = true,
            Order = 1,
            OperationalState = OperationalState.UNAVAILABLE,
            OperationalMessage = "Aguardando atualização do prontuário.",
            RowVersion = current!.RowVersion,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Put_ModuleFromOtherInstitution_ReturnsNotFound()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (institutionAId, adminAId) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        var (institutionBId, _) = await TestIdentitySeeder.SeedFullAccessUserAsync(db);
        await ProvisionAndGetCareIdAsync(institutionAId);
        var careIdInB = await ProvisionAndGetCareIdAsync(institutionBId);

        var clientA = _factory.CreateClient().AsUser(adminAId);
        var response = await clientA.PutAsJsonAsync($"/api/v1/AdminInstitutionModule/{careIdInB}", new InstitutionModuleUpdateRequest
        {
            IsEnabled = true,
            Order = 1,
            OperationalState = OperationalState.AVAILABLE,
            RowVersion = 0,
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
