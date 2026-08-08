using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

// Autoridade final de decisão de acesso (spec §5). Precedência fixa, sem cache — cada
// chamada lê o estado atual do banco.
public class AccessDecisionService : IAccessDecisionService
{
    private const int MinPermanentJustificationLength = 10;

    private readonly AppDbContext _dbContext;

    public AccessDecisionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccessDecision> EvaluateAsync(
        Guid actingUserId,
        string resource,
        string action,
        string? feature = null,
        AccessScopeType? targetScopeType = null,
        string? targetScopeKey = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Contexto/conta inválidos.
        var user = await _dbContext.Users.FindAsync(new object[] { actingUserId }, cancellationToken);
        if (user == null || user.AccountState != AccountState.ACTIVE)
            return Deny("INVALID_CONTEXT");

        var now = DateTime.UtcNow;

        // 2. Bypass restrito de SYSTEM_ADMIN — só para permissões marcadas como operação
        // de sistema; nunca para dados de negócio.
        if (user.IsSystemAdmin)
        {
            var isSystemOperation = await _dbContext.Permissions.AnyAsync(
                p => p.Resource == resource && p.Action == action && p.Feature == feature && p.IsSystemOperation,
                cancellationToken);
            if (isSystemOperation)
                return Allow("SYSTEM_ADMIN");
        }

        var overrides = _dbContext.UserPermissionOverrides.Where(o =>
            o.UserId == actingUserId &&
            o.Resource == resource &&
            o.Action == action &&
            (o.Feature == null || o.Feature == feature) &&
            o.ValidFrom <= now &&
            (o.ValidTo == null || o.ValidTo >= now) &&
            (o.ScopeType == null || targetScopeType == null || o.ScopeType == targetScopeType) &&
            (o.ScopeKey == null || targetScopeKey == null || o.ScopeKey == targetScopeKey));

        var policies = _dbContext.AccessPolicies.Where(p =>
            p.InstitutionId == user.InstitutionId &&
            p.State == AccessPolicyState.ACTIVE &&
            p.Resource == resource &&
            p.Action == action &&
            (p.Feature == null || p.Feature == feature) &&
            (p.ScopeType == null || targetScopeType == null || p.ScopeType == targetScopeType) &&
            (p.ScopeKey == null || targetScopeKey == null || p.ScopeKey == targetScopeKey));

        // 3. DENY individual.
        if (await overrides.AnyAsync(o => o.Effect == AccessEffect.DENY, cancellationToken))
            return Deny("DENY_INDIVIDUAL");

        // 4. Política condicional de negação.
        if (await policies.AnyAsync(p => p.Effect == AccessEffect.DENY, cancellationToken))
            return Deny("DENY_POLICY");

        // 5. ALLOW individual.
        if (await overrides.AnyAsync(o => o.Effect == AccessEffect.ALLOW, cancellationToken))
            return Allow("ALLOW_INDIVIDUAL");

        // 6. Política condicional de concessão.
        if (await policies.AnyAsync(p => p.Effect == AccessEffect.ALLOW, cancellationToken))
            return Allow("ALLOW_POLICY");

        // 7. RBAC — união de grupos alcançados por papel técnico direto e por
        // responsabilidade organizacional válida (respeitando escopo do alvo).
        var directGroupIds = _dbContext.UserRoles
            .Where(ur => ur.UserId == actingUserId)
            .Join(_dbContext.RolePermissionGroups, ur => ur.RoleId, rpg => rpg.RoleId, (ur, rpg) => rpg.PermissionGroupId);

        var validAssignments = _dbContext.OrganizationalRoleAssignments.Where(a =>
            a.UserId == actingUserId &&
            a.ValidFrom <= now &&
            (a.ValidTo == null || a.ValidTo >= now) &&
            (targetScopeType == null || a.ScopeType == AccessScopeType.INSTITUTION ||
                (a.ScopeType == targetScopeType && (a.ScopeKey == null || a.ScopeKey == targetScopeKey))));

        var orgGroupIds = validAssignments
            .Join(_dbContext.OrganizationalRolePermissionGroups, a => a.OrganizationalRoleId, g => g.OrganizationalRoleId, (a, g) => g.PermissionGroupId);

        var reachableGroupIds = directGroupIds.Union(orgGroupIds);

        var reachablePermissionIds = _dbContext.PermissionGroupPermissions
            .Where(pgp => reachableGroupIds.Contains(pgp.PermissionGroupId))
            .Select(pgp => pgp.PermissionId);

        var hasRbacGrant = await _dbContext.Permissions.AnyAsync(
            p => reachablePermissionIds.Contains(p.Id) && p.Resource == resource && p.Action == action && p.Feature == feature,
            cancellationToken);

        if (hasRbacGrant)
            return Allow("RBAC");

        // 8. Negação padrão.
        return Deny("DEFAULT_DENY");
    }

    public void ValidateOverrideJustification(string? justification, DateTime? validTo)
    {
        if (validTo != null)
            return;

        if (string.IsNullOrWhiteSpace(justification) || justification.Trim().Length < MinPermanentJustificationLength)
            throw new BusinessRuleException(
                $"Exceções permanentes exigem justificativa com pelo menos {MinPermanentJustificationLength} caracteres.");
    }

    private static AccessDecision Allow(string layer) => new() { Allowed = true, DeterminingLayer = layer };

    private static AccessDecision Deny(string layer) => new() { Allowed = false, DeterminingLayer = layer };
}
