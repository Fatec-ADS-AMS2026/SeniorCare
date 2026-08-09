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

// Atribuições de responsabilidade organizacional (§6.3): criar e encerrar antecipadamente
// (ValidTo = agora). Escopo e validade vêm da própria linha (§5's OrganizationalRoleAssignment).
[ApiController]
[Route("api/v1/[controller]")]
public class AdminOrganizationalRoleAssignmentController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminOrganizationalRoleAssignmentController(AppDbContext dbContext, ICurrentUserContext currentUserContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    [HttpGet]
    [RequirePermission("OrganizationalRoleAssignment", "read")]
    public async Task<ActionResult<PagedResult<OrganizationalRoleAssignmentDTO>>> Get([FromQuery] CatalogQuery query, [FromQuery] Guid? userId)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var institutionRoleIds = await _dbContext.OrganizationalRoles
            .Where(r => r.InstitutionId == institutionId)
            .Select(r => r.Id)
            .ToListAsync();

        var assignments = await _dbContext.OrganizationalRoleAssignments
            .Where(a => institutionRoleIds.Contains(a.OrganizationalRoleId) && (userId == null || a.UserId == userId))
            .ToListAsync();

        return Ok(assignments.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("OrganizationalRoleAssignment", "read")]
    public async Task<ActionResult<OrganizationalRoleAssignmentDTO>> GetById(Guid id)
    {
        var assignment = await GetInInstitutionAsync(id);
        return Ok(ToDto(assignment));
    }

    [HttpPost]
    [RequirePermission("OrganizationalRoleAssignment", "write")]
    public async Task<ActionResult<OrganizationalRoleAssignmentDTO>> Post(OrganizationalRoleAssignmentCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();

        var userBelongsToInstitution = await _dbContext.Users.AnyAsync(u => u.Id == request.UserId && u.InstitutionId == institutionId);
        if (!userBelongsToInstitution)
            throw new BusinessRuleException($"UserId {request.UserId} não referencia uma conta desta instituição.");

        var roleBelongsToInstitution = await _dbContext.OrganizationalRoles
            .AnyAsync(r => r.Id == request.OrganizationalRoleId && r.InstitutionId == institutionId);
        if (!roleBelongsToInstitution)
            throw new BusinessRuleException($"OrganizationalRoleId {request.OrganizationalRoleId} não referencia uma responsabilidade desta instituição.");

        var assignment = new OrganizationalRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            OrganizationalRoleId = request.OrganizationalRoleId,
            ScopeType = request.ScopeType,
            ScopeKey = request.ScopeKey,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
        };
        _dbContext.OrganizationalRoleAssignments.Add(assignment);
        await _dbContext.SaveChangesAsync();
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "OrganizationalRoleAssignment", "Create", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: institutionId, targetUserId: assignment.UserId,
            targetScopeType: assignment.ScopeType, targetScopeKey: assignment.ScopeKey,
            beforeValue: null, afterValue: new { assignment.Id, assignment.OrganizationalRoleId, assignment.ValidFrom, assignment.ValidTo });

        return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, ToDto(assignment));
    }

    [HttpPut("{id}/end")]
    [RequirePermission("OrganizationalRoleAssignment", "write")]
    public async Task<ActionResult<OrganizationalRoleAssignmentDTO>> End(Guid id)
    {
        var assignment = await GetInInstitutionAsync(id);
        var previousValidTo = assignment.ValidTo;
        var now = DateTime.UtcNow;
        if (assignment.ValidTo == null || assignment.ValidTo > now)
            assignment.ValidTo = now;
        await _dbContext.SaveChangesAsync();
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "OrganizationalRoleAssignment", "End", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: await _currentUserContext.GetInstitutionIdAsync(), targetUserId: assignment.UserId,
            beforeValue: new { ValidTo = previousValidTo }, afterValue: new { assignment.ValidTo });
        return Ok(ToDto(assignment));
    }

    private async Task<OrganizationalRoleAssignment> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var assignment = await _dbContext.OrganizationalRoleAssignments.SingleOrDefaultAsync(a => a.Id == id);
        if (assignment == null) throw new KeyNotFoundException("Atribuição não encontrada.");

        var roleBelongsToInstitution = await _dbContext.OrganizationalRoles
            .AnyAsync(r => r.Id == assignment.OrganizationalRoleId && r.InstitutionId == institutionId);
        if (!roleBelongsToInstitution) throw new KeyNotFoundException("Atribuição não encontrada.");

        return assignment;
    }

    private static OrganizationalRoleAssignmentDTO ToDto(OrganizationalRoleAssignment assignment) => new()
    {
        Id = assignment.Id,
        UserId = assignment.UserId,
        OrganizationalRoleId = assignment.OrganizationalRoleId,
        ScopeType = assignment.ScopeType,
        ScopeKey = assignment.ScopeKey,
        ValidFrom = assignment.ValidFrom,
        ValidTo = assignment.ValidTo,
    };
}
