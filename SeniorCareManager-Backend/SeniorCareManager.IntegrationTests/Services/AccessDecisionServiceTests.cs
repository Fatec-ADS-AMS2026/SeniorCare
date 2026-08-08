using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SeniorCareManager.IntegrationTests.Infrastructure;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.IntegrationTests.Services;

// Matriz de precedência (5.12) — cada teste seeda seu próprio usuário/instituição
// isolados, já que não há reset de banco entre testes na mesma classe.
public sealed class AccessDecisionServiceTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;

    public AccessDecisionServiceTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Evaluate_NoGrantsAtAll_DeniesByDefault()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (_, userId) = await CreateActiveUserAsync(db);

        var decision = await service.EvaluateAsync(userId, "ProductGroup", "read");

        decision.Allowed.Should().BeFalse();
        decision.DeterminingLayer.Should().Be("DEFAULT_DENY");
    }

    [Fact]
    public async Task Evaluate_InactiveAccount_AlwaysDenies()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (institutionId, userId) = await CreateActiveUserAsync(db);
        await GrantFullRbacAsync(db, institutionId, userId, "ProductGroup", "read");

        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.AccountState = AccountState.INACTIVE;
        await db.SaveChangesAsync();

        var decision = await service.EvaluateAsync(userId, "ProductGroup", "read");

        decision.Allowed.Should().BeFalse();
        decision.DeterminingLayer.Should().Be("INVALID_CONTEXT");
    }

    [Fact]
    public async Task Evaluate_RbacGrant_AllowsOnlyGrantedResource()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (institutionId, userId) = await CreateActiveUserAsync(db);
        await GrantFullRbacAsync(db, institutionId, userId, "ProductGroup", "read");

        var granted = await service.EvaluateAsync(userId, "ProductGroup", "read");
        var notGranted = await service.EvaluateAsync(userId, "ProductGroup", "delete");

        granted.Allowed.Should().BeTrue();
        granted.DeterminingLayer.Should().Be("RBAC");
        notGranted.Allowed.Should().BeFalse();
        notGranted.DeterminingLayer.Should().Be("DEFAULT_DENY");
    }

    [Fact]
    public async Task Evaluate_IndividualDenyOverridesIndividualAllow()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (_, userId) = await CreateActiveUserAsync(db);

        AddOverride(db, userId, "ProductGroup", "read", AccessEffect.ALLOW, userId);
        AddOverride(db, userId, "ProductGroup", "read", AccessEffect.DENY, userId);
        await db.SaveChangesAsync();

        var decision = await service.EvaluateAsync(userId, "ProductGroup", "read");

        decision.Allowed.Should().BeFalse();
        decision.DeterminingLayer.Should().Be("DENY_INDIVIDUAL");
    }

    [Fact]
    public async Task Evaluate_PolicyDenyOverridesIndividualAllow()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (institutionId, userId) = await CreateActiveUserAsync(db);

        AddOverride(db, userId, "ProductGroup", "read", AccessEffect.ALLOW, userId);
        db.AccessPolicies.Add(new AccessPolicy
        {
            Id = Guid.NewGuid(),
            PolicyKey = Guid.NewGuid(),
            Version = 1,
            InstitutionId = institutionId,
            Resource = "ProductGroup",
            Action = "read",
            Effect = AccessEffect.DENY,
            State = AccessPolicyState.ACTIVE,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = userId
        });
        await db.SaveChangesAsync();

        var decision = await service.EvaluateAsync(userId, "ProductGroup", "read");

        decision.Allowed.Should().BeFalse();
        decision.DeterminingLayer.Should().Be("DENY_POLICY");
    }

    [Fact]
    public async Task Evaluate_OrganizationalRoleAssignment_RespectsSectorScope()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (institutionId, userId) = await CreateActiveUserAsync(db);

        var group = new PermissionGroup { Id = Guid.NewGuid(), Name = $"Grupo {Guid.NewGuid():N}" };
        db.PermissionGroups.Add(group);
        var permissionId = await db.Permissions
            .Where(p => p.Resource == "ProductGroup" && p.Action == "read")
            .Select(p => p.Id)
            .SingleAsync();
        db.PermissionGroupPermissions.Add(new PermissionGroupPermission { PermissionGroupId = group.Id, PermissionId = permissionId });

        var orgRole = new OrganizationalRole { Id = Guid.NewGuid(), InstitutionId = institutionId, Name = $"Responsabilidade {Guid.NewGuid():N}" };
        db.OrganizationalRoles.Add(orgRole);
        db.OrganizationalRolePermissionGroups.Add(new OrganizationalRolePermissionGroup { OrganizationalRoleId = orgRole.Id, PermissionGroupId = group.Id });

        db.OrganizationalRoleAssignments.Add(new OrganizationalRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationalRoleId = orgRole.Id,
            ScopeType = AccessScopeType.SECTOR,
            ScopeKey = "SetorA",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = null
        });
        await db.SaveChangesAsync();

        var sameSector = await service.EvaluateAsync(userId, "ProductGroup", "read", targetScopeType: AccessScopeType.SECTOR, targetScopeKey: "SetorA");
        var otherSector = await service.EvaluateAsync(userId, "ProductGroup", "read", targetScopeType: AccessScopeType.SECTOR, targetScopeKey: "SetorB");

        sameSector.Allowed.Should().BeTrue();
        sameSector.DeterminingLayer.Should().Be("RBAC");
        otherSector.Allowed.Should().BeFalse();
        otherSector.DeterminingLayer.Should().Be("DEFAULT_DENY");
    }

    [Fact]
    public async Task Evaluate_ExpiredOrganizationalRoleAssignment_DoesNotGrant()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (institutionId, userId) = await CreateActiveUserAsync(db);

        var group = new PermissionGroup { Id = Guid.NewGuid(), Name = $"Grupo {Guid.NewGuid():N}" };
        db.PermissionGroups.Add(group);
        var permissionId = await db.Permissions
            .Where(p => p.Resource == "ProductGroup" && p.Action == "read")
            .Select(p => p.Id)
            .SingleAsync();
        db.PermissionGroupPermissions.Add(new PermissionGroupPermission { PermissionGroupId = group.Id, PermissionId = permissionId });

        var orgRole = new OrganizationalRole { Id = Guid.NewGuid(), InstitutionId = institutionId, Name = $"Responsabilidade {Guid.NewGuid():N}" };
        db.OrganizationalRoles.Add(orgRole);
        db.OrganizationalRolePermissionGroups.Add(new OrganizationalRolePermissionGroup { OrganizationalRoleId = orgRole.Id, PermissionGroupId = group.Id });

        db.OrganizationalRoleAssignments.Add(new OrganizationalRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationalRoleId = orgRole.Id,
            ScopeType = AccessScopeType.INSTITUTION,
            ValidFrom = DateTime.UtcNow.AddDays(-30),
            ValidTo = DateTime.UtcNow.AddDays(-1)
        });
        await db.SaveChangesAsync();

        var decision = await service.EvaluateAsync(userId, "ProductGroup", "read");

        decision.Allowed.Should().BeFalse();
        decision.DeterminingLayer.Should().Be("DEFAULT_DENY");
    }

    [Fact]
    public async Task Evaluate_SystemAdmin_OnlyBypassesSystemOperationPermissions()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();
        var (_, userId) = await CreateActiveUserAsync(db);

        var user = await db.Users.SingleAsync(u => u.Id == userId);
        user.IsSystemAdmin = true;

        var systemPermission = new Permission(Guid.NewGuid(), "SystemDiagnostics", "run", isSystemOperation: true);
        db.Permissions.Add(systemPermission);
        await db.SaveChangesAsync();

        var systemOperation = await service.EvaluateAsync(userId, "SystemDiagnostics", "run");
        var businessData = await service.EvaluateAsync(userId, "ProductGroup", "delete");

        systemOperation.Allowed.Should().BeTrue();
        systemOperation.DeterminingLayer.Should().Be("SYSTEM_ADMIN");
        businessData.Allowed.Should().BeFalse();
        businessData.DeterminingLayer.Should().Be("DEFAULT_DENY");
    }

    [Fact]
    public void ValidateOverrideJustification_PermanentWithoutJustification_Throws()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();

        var act = () => service.ValidateOverrideJustification(" ", null);

        act.Should().Throw<SeniorCareManager.WebAPI.Infrastructure.BusinessRuleException>();
    }

    [Fact]
    public void ValidateOverrideJustification_TemporaryWithoutJustification_DoesNotThrow()
    {
        using var scope = _factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAccessDecisionService>();

        var act = () => service.ValidateOverrideJustification(null, DateTime.UtcNow.AddDays(30));

        act.Should().NotThrow();
    }

    private static async Task<(Guid InstitutionId, Guid UserId)> CreateActiveUserAsync(AppDbContext db)
    {
        var institution = new Institution(Guid.NewGuid(), $"ILPI {Guid.NewGuid():N}");
        db.Institutions.Add(institution);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"user-{Guid.NewGuid():N}@example.com",
            Email = $"user-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Usuário de Teste",
            AccountState = AccountState.ACTIVE
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (institution.Id, user.Id);
    }

    private static async Task GrantFullRbacAsync(AppDbContext db, Guid institutionId, Guid userId, string resource, string action)
    {
        var group = new PermissionGroup { Id = Guid.NewGuid(), Name = $"Grupo {Guid.NewGuid():N}" };
        db.PermissionGroups.Add(group);

        var permissionId = await db.Permissions
            .Where(p => p.Resource == resource && p.Action == action)
            .Select(p => p.Id)
            .SingleAsync();
        db.PermissionGroupPermissions.Add(new PermissionGroupPermission { PermissionGroupId = group.Id, PermissionId = permissionId });

        var role = new Role { Id = Guid.NewGuid(), InstitutionId = institutionId, Name = $"Papel {Guid.NewGuid():N}" };
        db.Roles.Add(role);
        db.RolePermissionGroups.Add(new RolePermissionGroup { RoleId = role.Id, PermissionGroupId = group.Id });
        db.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = role.Id, CreatedAtUtc = DateTime.UtcNow });

        await db.SaveChangesAsync();
    }

    private static void AddOverride(AppDbContext db, Guid userId, string resource, string action, AccessEffect effect, Guid grantedByUserId)
    {
        db.UserPermissionOverrides.Add(new UserPermissionOverride
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Resource = resource,
            Action = action,
            Effect = effect,
            Justification = "Justificativa de teste com pelo menos dez caracteres.",
            GrantedByUserId = grantedByUserId,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(30),
            CreatedAtUtc = DateTime.UtcNow
        });
    }
}
