using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

// Contas administradas (§6.1): listar, criar, ativar/inativar/bloquear/expirar — sem
// exclusão física (o ciclo de vida é por estado) e sem nunca expor credenciais.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminUserController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminUserService _adminUserService;
    private readonly IAdminInvariantService _adminInvariantService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISessionService _sessionService;
    private readonly IAuditService _auditService;

    public AdminUserController(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IAdminUserService adminUserService,
        IAdminInvariantService adminInvariantService,
        ICurrentUserContext currentUserContext,
        ISessionService sessionService,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _adminUserService = adminUserService;
        _adminInvariantService = adminInvariantService;
        _currentUserContext = currentUserContext;
        _sessionService = sessionService;
        _auditService = auditService;
    }

    [HttpGet]
    [RequirePermission("AdminUser", "read")]
    public async Task<ActionResult<PagedResult<AdminUserDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var users = await _dbContext.Users.Where(u => u.InstitutionId == institutionId).ToListAsync();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? users
            : users.Where(u =>
                (u.DisplayName ?? string.Empty).Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                (u.Email ?? string.Empty).Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("AdminUser", "read")]
    public async Task<ActionResult<AdminUserDTO>> GetById(Guid id)
    {
        var user = await GetInInstitutionAsync(id);
        return Ok(ToDto(user));
    }

    [HttpPost]
    [RequirePermission("AdminUser", "write")]
    public async Task<ActionResult<AdminUserDTO>> Post(AdminUserCreateRequest request)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var (userId, _) = await _adminUserService.CreateAsync(institutionId, request.Email, request.DisplayName);
        var created = await _dbContext.Users.SingleAsync(u => u.Id == userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
    }

    [HttpPut("{id}/state")]
    [RequirePermission("AdminUser", "write")]
    public async Task<ActionResult<AdminUserDTO>> ChangeState(Guid id, AdminUserStateChangeRequest request)
    {
        var actingUserId = _currentUserContext.UserId;
        var actingUser = await _userManager.FindByIdAsync(actingUserId.ToString())
            ?? throw new BusinessRuleException("Identidade autenticada inválida.");
        if (!await _userManager.CheckPasswordAsync(actingUser, request.CurrentPassword))
            throw new BusinessRuleException("Senha atual inválida.");

        var target = await GetInInstitutionAsync(id);
        var previousState = target.AccountState;

        if (request.AccountState != AccountState.ACTIVE)
        {
            var institutionId = await _currentUserContext.GetInstitutionIdAsync();
            await _adminInvariantService.EnsureNotLastActiveAdministratorAsync(institutionId, target.Id);
        }

        target.AccountState = request.AccountState;
        await _userManager.UpdateAsync(target);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "AdminUser", "ChangeState", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: target.InstitutionId, targetUserId: target.Id,
            beforeValue: new { AccountState = previousState }, afterValue: new { AccountState = target.AccountState });

        // Sessões abertas deixam de autorizar novas requisições assim que a conta sai de
        // ACTIVE (§4/§7.3) — inclui reativação futura: quem reativa precisa logar de novo.
        if (request.AccountState != AccountState.ACTIVE)
        {
            await _sessionService.RevokeAllForUserAsync(target.Id);
            await _auditService.RecordAsync(AuditEventCategory.SESSION, "UserSession", "RevokeAll", AuditOutcome.SUCCESS,
                actorUserId: _currentUserContext.UserId, institutionId: target.InstitutionId, targetUserId: target.Id,
                description: "Efeito colateral de mudança de estado de conta.");
        }

        return Ok(ToDto(target));
    }

    private async Task<ApplicationUser> GetInInstitutionAsync(Guid id)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Id == id && u.InstitutionId == institutionId);
        if (user == null) throw new KeyNotFoundException("Conta não encontrada.");
        return user;
    }

    private static AdminUserDTO ToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        DisplayName = user.DisplayName,
        AccountState = user.AccountState,
        IdentityOrigin = user.IdentityOrigin,
    };
}
