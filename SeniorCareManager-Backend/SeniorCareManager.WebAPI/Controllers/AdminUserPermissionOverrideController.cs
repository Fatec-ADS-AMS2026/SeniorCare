using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Exceções individuais ALLOW/DENY (§6.4). Justificativa não-trivial exigida para exceção
// permanente (ValidTo == null) — reusa a validação já modelada na §5.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminUserPermissionOverrideController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly IAuditService _auditService;

    public AdminUserPermissionOverrideController(
        AppDbContext dbContext, ICurrentUserContext currentUserContext, IAccessDecisionService accessDecisionService,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _accessDecisionService = accessDecisionService;
        _auditService = auditService;
    }

    [HttpGet]
    [RequirePermission("UserPermissionOverride", "read")]
    public async Task<ActionResult<PagedResult<UserPermissionOverrideDTO>>> Get([FromQuery] CatalogQuery query, [FromQuery] Guid? userId)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var institutionUserIds = await _dbContext.Users
            .Where(u => u.InstitutionId == institutionId)
            .Select(u => u.Id)
            .ToListAsync();

        var overrides = await _dbContext.UserPermissionOverrides
            .Where(o => institutionUserIds.Contains(o.UserId) && (userId == null || o.UserId == userId))
            .ToListAsync();

        return Ok(overrides.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("UserPermissionOverride", "read")]
    public async Task<ActionResult<UserPermissionOverrideDTO>> GetById(Guid id)
    {
        var over = await GetInInstitutionAsync(id);
        return Ok(ToDto(over));
    }

    [HttpPost]
    [RequirePermission("UserPermissionOverride", "write")]
    public async Task<ActionResult<UserPermissionOverrideDTO>> Post(UserPermissionOverrideCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var userBelongsToInstitution = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId && u.InstitutionId == institutionId);
        if (!userBelongsToInstitution)
            throw new BusinessRuleException($"UserId {request.UserId} não referencia uma conta desta instituição.");

        _accessDecisionService.ValidateOverrideJustification(request.Justification, request.ValidTo);

        var over = new UserPermissionOverride
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Resource = request.Resource,
            Action = request.Action,
            Feature = request.Feature,
            ScopeType = request.ScopeType,
            ScopeKey = request.ScopeKey,
            Effect = request.Effect,
            Justification = request.Justification ?? string.Empty,
            GrantedByUserId = _currentUserContext.UserId,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _dbContext.UserPermissionOverrides.Add(over);
        await _dbContext.SaveChangesAsync();
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "UserPermissionOverride", "Create", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: institutionId, targetUserId: over.UserId,
            targetScopeType: over.ScopeType, targetScopeKey: over.ScopeKey,
            beforeValue: null,
            afterValue: new { over.Id, over.Resource, over.Action, over.Feature, over.Effect, over.ValidFrom, over.ValidTo });

        return CreatedAtAction(nameof(GetById), new { id = over.Id }, ToDto(over));
    }

    [HttpPut("{id}/revoke")]
    [RequirePermission("UserPermissionOverride", "write")]
    public async Task<ActionResult<UserPermissionOverrideDTO>> Revoke(Guid id)
    {
        var over = await GetInInstitutionAsync(id);
        var previousValidTo = over.ValidTo;
        var now = DateTime.UtcNow;
        if (over.ValidTo == null || over.ValidTo > now)
            over.ValidTo = now;
        await _dbContext.SaveChangesAsync();
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "UserPermissionOverride", "Revoke", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: await _currentUserContext.GetInstitutionIdAsync(), targetUserId: over.UserId,
            beforeValue: new { ValidTo = previousValidTo }, afterValue: new { over.ValidTo });
        return Ok(ToDto(over));
    }

    private async Task<UserPermissionOverride> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var over = await _dbContext.UserPermissionOverrides.SingleOrDefaultAsync(o => o.Id == id);
        if (over == null) throw new KeyNotFoundException("Exceção não encontrada.");

        var userBelongsToInstitution = await _dbContext.Users.AnyAsync(u => u.Id == over.UserId && u.InstitutionId == institutionId);
        if (!userBelongsToInstitution) throw new KeyNotFoundException("Exceção não encontrada.");

        return over;
    }

    private static UserPermissionOverrideDTO ToDto(UserPermissionOverride over) => new()
    {
        Id = over.Id,
        UserId = over.UserId,
        Resource = over.Resource,
        Action = over.Action,
        Feature = over.Feature,
        ScopeType = over.ScopeType,
        ScopeKey = over.ScopeKey,
        Effect = over.Effect,
        Justification = over.Justification,
        GrantedByUserId = over.GrantedByUserId,
        ValidFrom = over.ValidFrom,
        ValidTo = over.ValidTo,
    };
}
