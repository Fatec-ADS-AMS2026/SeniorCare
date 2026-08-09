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
using PermissionModel = SeniorCareManager.WebAPI.Objects.Models.Permission;

namespace SeniorCareManager.WebAPI.Controllers;

// Permission é o vocabulário fixo do sistema (§5) — só leitura, nunca criado por API.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminPermissionController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminPermissionController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [RequirePermission("Permission", "read")]
    public async Task<ActionResult<PagedResult<PermissionDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var permissions = await _dbContext.Permissions.ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? permissions
            : permissions.Where(p => p.Resource.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("Permission", "read")]
    public async Task<ActionResult<PermissionDTO>> GetById(Guid id)
    {
        var permission = await _dbContext.Permissions.SingleOrDefaultAsync(p => p.Id == id);
        if (permission == null) throw new KeyNotFoundException("Permissão não encontrada.");
        return Ok(ToDto(permission));
    }

    private static PermissionDTO ToDto(PermissionModel permission) => new()
    {
        Id = permission.Id,
        Resource = permission.Resource,
        Action = permission.Action,
        Feature = permission.Feature,
        Description = permission.Description,
        IsSystemOperation = permission.IsSystemOperation,
    };
}
