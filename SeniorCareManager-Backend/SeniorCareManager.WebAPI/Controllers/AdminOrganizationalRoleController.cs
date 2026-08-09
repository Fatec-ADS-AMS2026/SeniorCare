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

// Responsabilidades organizacionais (§6.3): CRUD por instituição + composição com
// PermissionGroup. Atribuição a usuários (com escopo/validade) fica em
// AdminOrganizationalRoleAssignmentController.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminOrganizationalRoleController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminOrganizationalRoleController(AppDbContext dbContext, ICurrentUserContext currentUserContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    private async Task RecordAsync(string action, Guid targetRoleInstitutionId, object? beforeValue, object? afterValue) =>
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "OrganizationalRole", action, AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: targetRoleInstitutionId,
            beforeValue: beforeValue, afterValue: afterValue);

    [HttpGet]
    [RequirePermission("OrganizationalRole", "read")]
    public async Task<ActionResult<PagedResult<OrganizationalRoleDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var roles = await _dbContext.OrganizationalRoles.Where(r => r.InstitutionId == institutionId).ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? roles
            : roles.Where(r => r.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("OrganizationalRole", "read")]
    public async Task<ActionResult<OrganizationalRoleDTO>> GetById(Guid id)
    {
        var role = await GetInInstitutionAsync(id);
        return Ok(ToDto(role));
    }

    [HttpPost]
    [RequirePermission("OrganizationalRole", "write")]
    public async Task<ActionResult<OrganizationalRoleDTO>> Post(OrganizationalRoleCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var role = new OrganizationalRole { Id = Guid.NewGuid(), InstitutionId = institutionId, Name = request.Name };
        _dbContext.OrganizationalRoles.Add(role);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Create", institutionId, beforeValue: null, afterValue: new { role.Id, role.Name });
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, ToDto(role));
    }

    [HttpPut("{id}")]
    [RequirePermission("OrganizationalRole", "write")]
    public async Task<ActionResult<OrganizationalRoleDTO>> Put(Guid id, OrganizationalRoleUpdateRequest request)
    {
        var role = await GetInInstitutionAsync(id);
        var previousName = role.Name;
        role.Name = request.Name;
        _dbContext.Entry(role).Property("Version").OriginalValue = request.RowVersion;
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Update", role.InstitutionId, beforeValue: new { Name = previousName }, afterValue: new { role.Name });
        return Ok(ToDto(role));
    }

    [HttpDelete("{id}")]
    [RequirePermission("OrganizationalRole", "delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await GetInInstitutionAsync(id);
        _dbContext.OrganizationalRoles.Remove(role);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Delete", role.InstitutionId, beforeValue: new { role.Id, role.Name }, afterValue: null);
        return NoContent();
    }

    [HttpPost("{id}/permission-groups")]
    [RequirePermission("OrganizationalRole", "write")]
    public async Task<IActionResult> AttachPermissionGroup(Guid id, PermissionGroupLinkRequest request)
    {
        var role = await GetInInstitutionAsync(id);
        var groupExists = await _dbContext.PermissionGroups.AnyAsync(g => g.Id == request.PermissionGroupId);
        if (!groupExists) throw new BusinessRuleException($"PermissionGroupId {request.PermissionGroupId} não referencia um grupo existente.");

        var alreadyLinked = await _dbContext.OrganizationalRolePermissionGroups
            .AnyAsync(x => x.OrganizationalRoleId == id && x.PermissionGroupId == request.PermissionGroupId);
        if (!alreadyLinked)
        {
            _dbContext.OrganizationalRolePermissionGroups.Add(new OrganizationalRolePermissionGroup
            {
                OrganizationalRoleId = id,
                PermissionGroupId = request.PermissionGroupId,
            });
            await _dbContext.SaveChangesAsync();
            await RecordAsync("AttachPermissionGroup", role.InstitutionId, beforeValue: null,
                afterValue: new { OrganizationalRoleId = id, request.PermissionGroupId });
        }

        return NoContent();
    }

    [HttpDelete("{id}/permission-groups/{permissionGroupId}")]
    [RequirePermission("OrganizationalRole", "write")]
    public async Task<IActionResult> DetachPermissionGroup(Guid id, Guid permissionGroupId)
    {
        var role = await GetInInstitutionAsync(id);
        var link = await _dbContext.OrganizationalRolePermissionGroups
            .SingleOrDefaultAsync(x => x.OrganizationalRoleId == id && x.PermissionGroupId == permissionGroupId);
        if (link != null)
        {
            _dbContext.OrganizationalRolePermissionGroups.Remove(link);
            await _dbContext.SaveChangesAsync();
            await RecordAsync("DetachPermissionGroup", role.InstitutionId,
                beforeValue: new { OrganizationalRoleId = id, PermissionGroupId = permissionGroupId }, afterValue: null);
        }

        return NoContent();
    }

    private async Task<OrganizationalRole> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var role = await _dbContext.OrganizationalRoles.SingleOrDefaultAsync(r => r.Id == id && r.InstitutionId == institutionId);
        if (role == null) throw new KeyNotFoundException("Responsabilidade organizacional não encontrada.");
        return role;
    }

    private OrganizationalRoleDTO ToDto(OrganizationalRole role) => new()
    {
        Id = role.Id,
        InstitutionId = role.InstitutionId,
        Name = role.Name,
        RowVersion = (uint)_dbContext.Entry(role).Property("Version").CurrentValue!,
    };
}
