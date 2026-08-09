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
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Controllers;

// Papéis técnicos (§6.2): CRUD por instituição + composição com PermissionGroup +
// atribuição a usuários (UserRole).
[ApiController]
[Route("api/v1/[controller]")]
public class AdminRoleController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserContext _currentUserContext;

    public AdminRoleController(AppDbContext dbContext, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _currentUserContext = currentUserContext;
    }

    [HttpGet]
    [RequirePermission("Role", "read")]
    public async Task<ActionResult<PagedResult<RoleDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var roles = await _dbContext.Roles.Where(r => r.InstitutionId == institutionId).ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? roles
            : roles.Where(r => r.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("Role", "read")]
    public async Task<ActionResult<RoleDTO>> GetById(Guid id)
    {
        var role = await GetInInstitutionAsync(id);
        return Ok(ToDto(role));
    }

    [HttpPost]
    [RequirePermission("Role", "write")]
    public async Task<ActionResult<RoleDTO>> Post(RoleCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var role = new Role { Id = Guid.NewGuid(), InstitutionId = institutionId, Name = request.Name };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, ToDto(role));
    }

    [HttpPut("{id}")]
    [RequirePermission("Role", "write")]
    public async Task<ActionResult<RoleDTO>> Put(Guid id, RoleUpdateRequest request)
    {
        var role = await GetInInstitutionAsync(id);
        role.Name = request.Name;
        _dbContext.Entry(role).Property("Version").OriginalValue = request.RowVersion;
        await _dbContext.SaveChangesAsync();
        return Ok(ToDto(role));
    }

    [HttpDelete("{id}")]
    [RequirePermission("Role", "delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var role = await GetInInstitutionAsync(id);
        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/permission-groups")]
    [RequirePermission("Role", "write")]
    public async Task<IActionResult> AttachPermissionGroup(Guid id, PermissionGroupLinkRequest request)
    {
        await GetInInstitutionAsync(id);
        await EnsurePermissionGroupExistsAsync(request.PermissionGroupId);

        var alreadyLinked = await _dbContext.RolePermissionGroups
            .AnyAsync(x => x.RoleId == id && x.PermissionGroupId == request.PermissionGroupId);
        if (!alreadyLinked)
        {
            _dbContext.RolePermissionGroups.Add(new RolePermissionGroup { RoleId = id, PermissionGroupId = request.PermissionGroupId });
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}/permission-groups/{permissionGroupId}")]
    [RequirePermission("Role", "write")]
    public async Task<IActionResult> DetachPermissionGroup(Guid id, Guid permissionGroupId)
    {
        await GetInInstitutionAsync(id);
        var link = await _dbContext.RolePermissionGroups
            .SingleOrDefaultAsync(x => x.RoleId == id && x.PermissionGroupId == permissionGroupId);
        if (link != null)
        {
            _dbContext.RolePermissionGroups.Remove(link);
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPost("{id}/users")]
    [RequirePermission("Role", "write")]
    public async Task<IActionResult> AssignToUser(Guid id, UserRoleAssignmentRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        await GetInInstitutionAsync(id);

        var targetUserBelongsToInstitution = await _dbContext.Users
            .AnyAsync(u => u.Id == request.UserId && u.InstitutionId == institutionId);
        if (!targetUserBelongsToInstitution)
            throw new BusinessRuleException($"UserId {request.UserId} não referencia uma conta desta instituição.");

        var alreadyAssigned = await _dbContext.UserRoles.AnyAsync(x => x.RoleId == id && x.UserId == request.UserId);
        if (!alreadyAssigned)
        {
            _dbContext.UserRoles.Add(new UserRole { Id = Guid.NewGuid(), UserId = request.UserId, RoleId = id, CreatedAtUtc = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id}/users/{userId}")]
    [RequirePermission("Role", "write")]
    public async Task<IActionResult> UnassignFromUser(Guid id, Guid userId)
    {
        await GetInInstitutionAsync(id);
        var assignment = await _dbContext.UserRoles.SingleOrDefaultAsync(x => x.RoleId == id && x.UserId == userId);
        if (assignment != null)
        {
            _dbContext.UserRoles.Remove(assignment);
            await _dbContext.SaveChangesAsync();
        }

        return NoContent();
    }

    private async Task<Role> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var role = await _dbContext.Roles.SingleOrDefaultAsync(r => r.Id == id && r.InstitutionId == institutionId);
        if (role == null) throw new KeyNotFoundException("Papel não encontrado.");
        return role;
    }

    private async Task EnsurePermissionGroupExistsAsync(Guid permissionGroupId)
    {
        var exists = await _dbContext.PermissionGroups.AnyAsync(g => g.Id == permissionGroupId);
        if (!exists) throw new BusinessRuleException($"PermissionGroupId {permissionGroupId} não referencia um grupo existente.");
    }

    private RoleDTO ToDto(Role role) => new()
    {
        Id = role.Id,
        InstitutionId = role.InstitutionId,
        Name = role.Name,
        RowVersion = (uint)_dbContext.Entry(role).Property("Version").CurrentValue!,
    };
}
