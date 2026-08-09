using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.IntegrationTests.Infrastructure;

/// <summary>
/// Fixtures de identidade/RBAC para testes que não são sobre autorização em si (ex.: CRUD de
/// catálogo) — "acesso total" é a fixture certa para esses; testes de precedência (5.12)
/// constroem grants parciais deliberadamente, sem usar este helper.
/// </summary>
public static class TestIdentitySeeder
{
    public static async Task<(Guid InstitutionId, Guid UserId)> SeedFullAccessUserAsync(AppDbContext db)
    {
        var institution = new Institution(Guid.NewGuid(), $"ILPI Teste {Guid.NewGuid():N}");
        db.Institutions.Add(institution);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"full-access-{Guid.NewGuid():N}@example.com",
            Email = $"full-access-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = "Usuário de Teste (acesso total)",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.ACTIVE,
            // UserManager.UpdateAsync exige SecurityStamp não-nulo — CreateAsync o gera
            // automaticamente, mas aqui o usuário é inserido direto via DbContext.
            SecurityStamp = Guid.NewGuid().ToString(),
            // Idem: CreateAsync normalmente liga isso (Options.Lockout.AllowedForNewUsers) —
            // sem isso, UserManager.IsLockedOutAsync sempre retorna false, mesmo com
            // AccessFailedCount/LockoutEnd definidos (§7.8).
            LockoutEnabled = true,
        };
        db.Users.Add(user);

        var group = new PermissionGroup { Id = Guid.NewGuid(), Name = $"Grupo de Teste {Guid.NewGuid():N}" };
        db.PermissionGroups.Add(group);

        var role = new Role { Id = Guid.NewGuid(), InstitutionId = institution.Id, Name = $"Papel de Teste {Guid.NewGuid():N}" };
        db.Roles.Add(role);

        db.RolePermissionGroups.Add(new RolePermissionGroup { RoleId = role.Id, PermissionGroupId = group.Id });
        db.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = user.Id, RoleId = role.Id, CreatedAtUtc = DateTime.UtcNow });

        var allPermissionIds = await db.Permissions.Select(p => p.Id).ToListAsync();
        foreach (var permissionId in allPermissionIds)
            db.PermissionGroupPermissions.Add(new PermissionGroupPermission { PermissionGroupId = group.Id, PermissionId = permissionId });

        await db.SaveChangesAsync();

        return (institution.Id, user.Id);
    }

    // Igual acima, mas com senha conhecida — necessário para os testes de reautenticação
    // (§6.7): CheckPasswordAsync exige que o usuário que executa a ação tenha credencial.
    public const string DefaultTestPassword = "Correto-Cavalo-Grampo-Teste-2026"; // gitleaks:allow — senha fixa de teste, não é segredo

    public static async Task<(Guid InstitutionId, Guid UserId)> SeedFullAccessUserWithPasswordAsync(
        AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        var (institutionId, userId) = await SeedFullAccessUserAsync(db);
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        await userManager.AddPasswordAsync(user, DefaultTestPassword);
        return (institutionId, userId);
    }

    // Conta ACTIVE sem nenhum Role/vínculo — todo RequirePermission deve negar (403) para
    // ela, salvo quando `institutionId` é passado (reaproveita instituição de outro seed,
    // para testar isolamento institucional).
    public static async Task<Guid> SeedNoGrantsUserAsync(AppDbContext db, Guid? institutionId = null)
    {
        var resolvedInstitutionId = institutionId;
        if (resolvedInstitutionId == null)
        {
            var institution = new Institution(Guid.NewGuid(), $"ILPI Teste {Guid.NewGuid():N}");
            db.Institutions.Add(institution);
            resolvedInstitutionId = institution.Id;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = $"sem-grant-{Guid.NewGuid():N}@example.com",
            Email = $"sem-grant-{Guid.NewGuid():N}@example.com",
            EmailConfirmed = true,
            InstitutionId = resolvedInstitutionId.Value,
            DisplayName = "Usuário de Teste (sem acesso)",
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.ACTIVE,
            SecurityStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user.Id;
    }

    // Igual a SeedNoGrantsUserAsync, mas com senha conhecida e sem a permissão marcadora
    // AccessAdministration/manage — necessário para testes de login/sessão (§7) que não devem
    // disparar MFA obrigatório (diferente de "acesso total", que a inclui de propósito).
    public static async Task<(Guid InstitutionId, Guid UserId)> SeedNoGrantsUserWithPasswordAsync(
        AppDbContext db, UserManager<ApplicationUser> userManager, Guid? institutionId = null)
    {
        var userId = await SeedNoGrantsUserAsync(db, institutionId);
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        await userManager.AddPasswordAsync(user, DefaultTestPassword);
        return (user.InstitutionId, userId);
    }
}
