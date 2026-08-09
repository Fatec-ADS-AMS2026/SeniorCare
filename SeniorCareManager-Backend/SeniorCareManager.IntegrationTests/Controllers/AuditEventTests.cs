using System.Linq;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Controllers;

public sealed class AuditEventTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AuditEventTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_RecordsAttributionCorrectly()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, userId) = await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login",
            new LoginRequest { Email = email, Password = TestIdentitySeeder.DefaultTestPassword });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var loginEvent = await freshDb.AuditEvents
            .Where(e => e.TargetUserId == userId && e.Category == AuditEventCategory.AUTHENTICATION && e.Action == "Login")
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstAsync();

        loginEvent.ActorUserId.Should().Be(userId);
        loginEvent.InstitutionId.Should().Be(institutionId);
        loginEvent.Outcome.Should().Be(AuditOutcome.SUCCESS);
        loginEvent.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ChangeState_ToInactive_RecordsTwoCorrelatedEvents()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (institutionId, adminId) = await TestIdentitySeeder.SeedFullAccessUserWithPasswordAsync(db, userManager);
        var targetUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db, institutionId);

        var client = _factory.CreateClient().AsUser(adminId);
        var response = await client.PutAsJsonAsync($"/api/v1/AdminUser/{targetUserId}/state",
            new AdminUserStateChangeRequest { AccountState = AccountState.INACTIVE, CurrentPassword = TestIdentitySeeder.DefaultTestPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var events = await freshDb.AuditEvents
            .Where(e => e.TargetUserId == targetUserId)
            .ToListAsync();

        var stateChangeEvent = events.Should().ContainSingle(e => e.Category == AuditEventCategory.AUTHENTICATION && e.Action == "ChangeState")
            .Subject;
        var sessionRevokeEvent = events.Should().ContainSingle(e => e.Category == AuditEventCategory.SESSION && e.Action == "RevokeAll")
            .Subject;

        // Mesma requisição HTTP produziu os dois — precisam compartilhar o CorrelationId
        // (HttpContext.TraceIdentifier), a prova de que a correlação (§8.2) funciona.
        stateChangeEvent.CorrelationId.Should().Be(sessionRevokeEvent.CorrelationId);
        stateChangeEvent.BeforeValue.Should().Contain("ACTIVE");
        stateChangeEvent.AfterValue.Should().Contain("INACTIVE");
    }

    [Fact]
    public async Task DirectDbUpdateOrDelete_OnAuditEvent_ThrowsViaInterceptor()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);

        var anyEvent = await db.AuditEvents.AsNoTracking().FirstAsync();

        anyEvent.Description = "tentativa de alterar um registro append-only";
        db.AuditEvents.Update(anyEvent);
        var updateAttempt = async () => await db.SaveChangesAsync();
        await updateAttempt.Should().ThrowAsync<InvalidOperationException>();

        db.ChangeTracker.Clear();
        var toDelete = await db.AuditEvents.FirstAsync();
        db.AuditEvents.Remove(toDelete);
        var deleteAttempt = async () => await db.SaveChangesAsync();
        await deleteAttempt.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DirectSqlUpdateOrDelete_OnAuditEvent_IsRejectedByPostgresTrigger()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);

        var anyEventId = await db.AuditEvents.Select(e => e.Id).FirstAsync();

        // Contorna o EF/ChangeTracker de propósito (SQL bruto direto) — prova a segunda
        // camada de imutabilidade (§8.6), a trigger no próprio Postgres, independente do
        // AuditImmutabilityInterceptor.
        var updateAttempt = async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE audit_event SET description = 'bypass' WHERE id = {anyEventId}");
        await updateAttempt.Should().ThrowAsync<PostgresException>();

        var deleteAttempt = async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM audit_event WHERE id = {anyEventId}");
        await deleteAttempt.Should().ThrowAsync<PostgresException>();
    }

    [Fact]
    public async Task RequirePermission_RecordsDeterminingLayer_ForDefaultDenyAndSystemAdmin()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var noGrantsUserId = await TestIdentitySeeder.SeedNoGrantsUserAsync(db);

        var deniedClient = _factory.CreateClient().AsUser(noGrantsUserId);
        var deniedResponse = await deniedClient.GetAsync("/api/v1/AdminRole");
        deniedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Nenhuma Permission do seed atual tem IsSystemOperation=true (o bypass está
        // modelado mas ainda não é usado por endpoint algum) — vira true só nesta linha,
        // só neste container de teste isolado, pra exercitar o caminho de verdade.
        var roleReadPermission = await db.Permissions.SingleAsync(p => p.Resource == "Role" && p.Action == "read");
        roleReadPermission.IsSystemOperation = true;
        await db.SaveChangesAsync();

        var systemAdminId = await SeedSystemAdminWithoutGrantsAsync(db);
        var systemAdminClient = _factory.CreateClient().AsUser(systemAdminId);
        var allowedResponse = await systemAdminClient.GetAsync("/api/v1/AdminRole");
        allowedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var deniedEvent = await freshDb.AuditEvents
            .Where(e => e.ActorUserId == noGrantsUserId && e.Category == AuditEventCategory.ACCESS_DECISION)
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstAsync();
        deniedEvent.Outcome.Should().Be(AuditOutcome.DENIED);
        deniedEvent.DeterminingLayer.Should().Be("DEFAULT_DENY");

        var allowedEvent = await freshDb.AuditEvents
            .Where(e => e.ActorUserId == systemAdminId && e.Category == AuditEventCategory.ACCESS_DECISION)
            .OrderByDescending(e => e.OccurredAtUtc)
            .FirstAsync();
        allowedEvent.Outcome.Should().Be(AuditOutcome.SUCCESS);
        allowedEvent.DeterminingLayer.Should().Be("SYSTEM_ADMIN");
    }

    [Fact]
    public async Task PasswordChangeFlow_NeverPersistsRawPasswordInAuditEvents()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var (_, userId) = await TestIdentitySeeder.SeedNoGrantsUserWithPasswordAsync(db, userManager);
        var email = (await db.Users.SingleAsync(u => u.Id == userId)).Email!;

        const string newPassword = "Outra-Senha-De-Teste-Bem-Longa-2026"; // gitleaks:allow — senha fixa de teste, não é segredo

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/Auth/change-password",
            new ChangePasswordRequest { Email = email, CurrentPassword = TestIdentitySeeder.DefaultTestPassword, NewPassword = newPassword });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var freshScope = _factory.Services.CreateScope();
        var freshDb = freshScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var events = await freshDb.AuditEvents.Where(e => e.TargetUserId == userId).ToListAsync();

        events.Should().NotBeEmpty();
        foreach (var auditEvent in events)
        {
            auditEvent.BeforeValue.Should().NotContain(TestIdentitySeeder.DefaultTestPassword).And.NotContain(newPassword);
            auditEvent.AfterValue.Should().NotContain(TestIdentitySeeder.DefaultTestPassword).And.NotContain(newPassword);
            auditEvent.Description.Should().NotContain(TestIdentitySeeder.DefaultTestPassword).And.NotContain(newPassword);
        }
    }

    // IsSystemAdmin=true e SEM nenhum grant — só assim uma permissão concedida prova que
    // veio do bypass SYSTEM_ADMIN (AccessDecisionService), não de RBAC comum.
    private static async Task<Guid> SeedSystemAdminWithoutGrantsAsync(AppDbContext db)
    {
        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);
        var email = $"sysadmin-{Guid.NewGuid():N}@example.com";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "System Admin de Teste",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.ACTIVE,
            IsSystemAdmin = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user.Id;
    }
}
