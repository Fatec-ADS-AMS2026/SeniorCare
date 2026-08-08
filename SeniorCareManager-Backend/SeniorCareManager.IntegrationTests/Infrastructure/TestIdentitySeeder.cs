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
            AccountState = AccountState.ACTIVE
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
}
