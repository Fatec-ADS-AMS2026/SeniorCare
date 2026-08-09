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

// PermissionGroup é global/sistema (§5) — não delimitado por instituição, ao contrário de
// Role/OrganizationalRole. CRUD + composição com Permission.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminPermissionGroupController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminPermissionGroupController(AppDbContext dbContext, ICurrentUserContext currentUserContext, IAuditService auditService)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    // PermissionGroup não tem InstitutionId (é global) — a auditoria ainda registra a
    // instituição de quem agiu, útil pra correlacionar "quem mexeu nisso" mesmo num recurso
    // sem escopo institucional.
    private async Task RecordAsync(string action, object? beforeValue, object? afterValue) =>
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "PermissionGroup", action, AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: await _currentUserContext.GetInstitutionIdAsync(),
            beforeValue: beforeValue, afterValue: afterValue);

    [HttpGet]
    [RequirePermission("PermissionGroup", "read")]
    public async Task<ActionResult<PagedResult<PermissionGroupDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var groups = await _dbContext.PermissionGroups.ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? groups
            : groups.Where(g => g.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("PermissionGroup", "read")]
    public async Task<ActionResult<PermissionGroupDTO>> GetById(Guid id)
    {
        var group = await GetOrThrowAsync(id);
        return Ok(ToDto(group));
    }

    [HttpPost]
    [RequirePermission("PermissionGroup", "write")]
    public async Task<ActionResult<PermissionGroupDTO>> Post(PermissionGroupCreateRequest request)
    {
        var group = new PermissionGroup { Id = Guid.NewGuid(), Name = request.Name };
        _dbContext.PermissionGroups.Add(group);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Create", beforeValue: null, afterValue: new { group.Id, group.Name });
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, ToDto(group));
    }

    [HttpPut("{id}")]
    [RequirePermission("PermissionGroup", "write")]
    public async Task<ActionResult<PermissionGroupDTO>> Put(Guid id, PermissionGroupUpdateRequest request)
    {
        var group = await GetOrThrowAsync(id);
        var previousName = group.Name;
        group.Name = request.Name;
        _dbContext.Entry(group).Property("Version").OriginalValue = request.RowVersion;
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Update", beforeValue: new { Name = previousName }, afterValue: new { group.Name });
        return Ok(ToDto(group));
    }

    [HttpDelete("{id}")]
    [RequirePermission("PermissionGroup", "delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var group = await GetOrThrowAsync(id);
        _dbContext.PermissionGroups.Remove(group);
        await _dbContext.SaveChangesAsync();
        await RecordAsync("Delete", beforeValue: new { group.Id, group.Name }, afterValue: null);
        return NoContent();
    }

    [HttpPost("{id}/permissions")]
    [RequirePermission("PermissionGroup", "write")]
    public async Task<IActionResult> AttachPermission(Guid id, PermissionLinkRequest request)
    {
        await GetOrThrowAsync(id);
        var permissionExists = await _dbContext.Permissions.AnyAsync(p => p.Id == request.PermissionId);
        if (!permissionExists) throw new BusinessRuleException($"PermissionId {request.PermissionId} não referencia uma permissão existente.");

        var alreadyLinked = await _dbContext.PermissionGroupPermissions
            .AnyAsync(x => x.PermissionGroupId == id && x.PermissionId == request.PermissionId);
        if (!alreadyLinked)
        {
            _dbContext.PermissionGroupPermissions.Add(new PermissionGroupPermission { PermissionGroupId = id, PermissionId = request.PermissionId });
            await _dbContext.SaveChangesAsync();
            await RecordAsync("AttachPermission", beforeValue: null, afterValue: new { PermissionGroupId = id, request.PermissionId });
        }

        return NoContent();
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    [RequirePermission("PermissionGroup", "write")]
    public async Task<IActionResult> DetachPermission(Guid id, Guid permissionId)
    {
        await GetOrThrowAsync(id);
        var link = await _dbContext.PermissionGroupPermissions
            .SingleOrDefaultAsync(x => x.PermissionGroupId == id && x.PermissionId == permissionId);
        if (link != null)
        {
            _dbContext.PermissionGroupPermissions.Remove(link);
            await _dbContext.SaveChangesAsync();
            await RecordAsync("DetachPermission", beforeValue: new { PermissionGroupId = id, PermissionId = permissionId }, afterValue: null);
        }

        return NoContent();
    }

    private async Task<PermissionGroup> GetOrThrowAsync(Guid id)
    {
        var group = await _dbContext.PermissionGroups.SingleOrDefaultAsync(g => g.Id == id);
        if (group == null) throw new KeyNotFoundException("Grupo de permissão não encontrado.");
        return group;
    }

    private PermissionGroupDTO ToDto(PermissionGroup group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        RowVersion = (uint)_dbContext.Entry(group).Property("Version").CurrentValue!,
    };
}
