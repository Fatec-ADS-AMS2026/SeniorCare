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

// Políticas condicionais (§6.4): versionadas/historizadas (§5) — editar cria uma nova linha
// (mesma PolicyKey, Version+1), nunca muta a existente.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminAccessPolicyController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminAccessPolicyController(AppDbContext dbContext, ICurrentUserContext currentUserContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    private async Task RecordAsync(string action, Guid institutionId, object? beforeValue, object? afterValue) =>
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "AccessPolicy", action, AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: institutionId, beforeValue: beforeValue, afterValue: afterValue);

    [HttpGet]
    [RequirePermission("AccessPolicy", "read")]
    public async Task<ActionResult<PagedResult<AccessPolicyDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var policies = await _dbContext.AccessPolicies.Where(p => p.InstitutionId == institutionId).ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? policies
            : policies.Where(p => p.Resource.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("AccessPolicy", "read")]
    public async Task<ActionResult<AccessPolicyDTO>> GetById(Guid id)
    {
        var policy = await GetInInstitutionAsync(id);
        return Ok(ToDto(policy));
    }

    [HttpPost]
    [RequirePermission("AccessPolicy", "write")]
    public async Task<ActionResult<AccessPolicyDTO>> Post(AccessPolicyCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var policy = new AccessPolicy
        {
            Id = Guid.NewGuid(),
            PolicyKey = Guid.NewGuid(),
            Version = 1,
            InstitutionId = institutionId,
            Resource = request.Resource,
            Action = request.Action,
            Feature = request.Feature,
            ScopeType = request.ScopeType,
            ScopeKey = request.ScopeKey,
            Effect = request.Effect,
            State = AccessPolicyState.DRAFT,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUserContext.UserId,
        };
        _dbContext.AccessPolicies.Add(policy);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Create", institutionId, beforeValue: null,
            afterValue: new { policy.Id, policy.PolicyKey, policy.Version, policy.Resource, policy.Action, policy.Effect });
        return CreatedAtAction(nameof(GetById), new { id = policy.Id }, ToDto(policy));
    }

    [HttpPost("{id}/revise")]
    [RequirePermission("AccessPolicy", "write")]
    public async Task<ActionResult<AccessPolicyDTO>> Revise(Guid id, AccessPolicyCreateRequest request)
    {
        var current = await GetInInstitutionAsync(id);
        var latestVersion = await _dbContext.AccessPolicies
            .Where(p => p.PolicyKey == current.PolicyKey)
            .MaxAsync(p => p.Version);

        var revision = new AccessPolicy
        {
            Id = Guid.NewGuid(),
            PolicyKey = current.PolicyKey,
            Version = latestVersion + 1,
            InstitutionId = current.InstitutionId,
            Resource = request.Resource,
            Action = request.Action,
            Feature = request.Feature,
            ScopeType = request.ScopeType,
            ScopeKey = request.ScopeKey,
            Effect = request.Effect,
            State = AccessPolicyState.DRAFT,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByUserId = _currentUserContext.UserId,
        };
        _dbContext.AccessPolicies.Add(revision);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Revise", current.InstitutionId,
            beforeValue: new { PolicyKey = current.PolicyKey, Version = latestVersion },
            afterValue: new { revision.Id, revision.PolicyKey, revision.Version, revision.Resource, revision.Action, revision.Effect });
        return CreatedAtAction(nameof(GetById), new { id = revision.Id }, ToDto(revision));
    }

    [HttpPut("{id}/activate")]
    [RequirePermission("AccessPolicy", "write")]
    public async Task<ActionResult<AccessPolicyDTO>> Activate(Guid id)
    {
        var policy = await GetInInstitutionAsync(id);
        if (policy.State != AccessPolicyState.DRAFT)
            throw new BusinessRuleException("Só uma política em rascunho pode ser ativada.");

        var previouslyActive = await _dbContext.AccessPolicies
            .Where(p => p.PolicyKey == policy.PolicyKey && p.State == AccessPolicyState.ACTIVE)
            .ToListAsync();
        foreach (var previous in previouslyActive)
            previous.State = AccessPolicyState.RETIRED;

        policy.State = AccessPolicyState.ACTIVE;
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Activate", policy.InstitutionId,
            beforeValue: new { State = AccessPolicyState.DRAFT, RetiredVersions = previouslyActive.Select(p => p.Version) },
            afterValue: new { policy.Id, policy.PolicyKey, policy.Version, State = AccessPolicyState.ACTIVE });
        return Ok(ToDto(policy));
    }

    [HttpPut("{id}/retire")]
    [RequirePermission("AccessPolicy", "write")]
    public async Task<ActionResult<AccessPolicyDTO>> Retire(Guid id)
    {
        var policy = await GetInInstitutionAsync(id);
        if (policy.State != AccessPolicyState.ACTIVE)
            throw new BusinessRuleException("Só uma política ativa pode ser aposentada.");

        policy.State = AccessPolicyState.RETIRED;
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Retire", policy.InstitutionId,
            beforeValue: new { State = AccessPolicyState.ACTIVE }, afterValue: new { policy.Id, policy.PolicyKey, policy.Version, State = AccessPolicyState.RETIRED });
        return Ok(ToDto(policy));
    }

    private async Task<AccessPolicy> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var policy = await _dbContext.AccessPolicies.SingleOrDefaultAsync(p => p.Id == id && p.InstitutionId == institutionId);
        if (policy == null) throw new KeyNotFoundException("Política não encontrada.");
        return policy;
    }

    private static AccessPolicyDTO ToDto(AccessPolicy policy) => new()
    {
        Id = policy.Id,
        PolicyKey = policy.PolicyKey,
        Version = policy.Version,
        Resource = policy.Resource,
        Action = policy.Action,
        Feature = policy.Feature,
        ScopeType = policy.ScopeType,
        ScopeKey = policy.ScopeKey,
        Effect = policy.Effect,
        State = policy.State,
    };
}
