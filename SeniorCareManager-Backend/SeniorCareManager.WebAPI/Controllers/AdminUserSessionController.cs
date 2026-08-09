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
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Sessões ativas (§6.6/§7.3). Revogar exige reautenticação (§6.7).
[ApiController]
[Route("api/v1/[controller]")]
public class AdminUserSessionController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISessionService _sessionService;

    public AdminUserSessionController(
        AppDbContext dbContext, UserManager<ApplicationUser> userManager, ICurrentUserContext currentUserContext, ISessionService sessionService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _currentUserContext = currentUserContext;
        _sessionService = sessionService;
    }

    [HttpGet]
    [RequirePermission("UserSession", "read")]
    public async Task<ActionResult<PagedResult<UserSessionDTO>>> Get([FromQuery] CatalogQuery query, [FromQuery] Guid userId)
    {
        await EnsureUserInInstitutionAsync(userId);
        var sessions = await _dbContext.UserSessions.Where(s => s.UserId == userId).ToListAsync();
        return Ok(sessions.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpPut("{id}/revoke")]
    [RequirePermission("UserSession", "write")]
    public async Task<ActionResult<UserSessionDTO>> Revoke(Guid id, RevokeSessionRequest request)
    {
        await CheckCurrentPasswordAsync(request.CurrentPassword);

        var session = await _dbContext.UserSessions.SingleOrDefaultAsync(s => s.Id == id);
        if (session == null) throw new KeyNotFoundException("Sessão não encontrada.");
        await EnsureUserInInstitutionAsync(session.UserId);

        await _sessionService.RevokeAsync(id);
        await _dbContext.Entry(session).ReloadAsync();
        return Ok(ToDto(session));
    }

    [HttpPut("revoke-all")]
    [RequirePermission("UserSession", "write")]
    public async Task<IActionResult> RevokeAll([FromQuery] Guid userId, RevokeSessionRequest request)
    {
        await CheckCurrentPasswordAsync(request.CurrentPassword);
        await EnsureUserInInstitutionAsync(userId);

        await _sessionService.RevokeAllForUserAsync(userId);

        return NoContent();
    }

    private async Task CheckCurrentPasswordAsync(string currentPassword)
    {
        var actingUser = await _userManager.FindByIdAsync(_currentUserContext.UserId.ToString())
            ?? throw new BusinessRuleException("Identidade autenticada inválida.");
        if (!await _userManager.CheckPasswordAsync(actingUser, currentPassword))
            throw new BusinessRuleException("Senha atual inválida.");
    }

    private async Task EnsureUserInInstitutionAsync(Guid userId)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        var belongs = await _dbContext.Users.AnyAsync(u => u.Id == userId && u.InstitutionId == institutionId);
        if (!belongs) throw new KeyNotFoundException("Conta não encontrada.");
    }

    private static UserSessionDTO ToDto(UserSession session) => new()
    {
        Id = session.Id,
        UserId = session.UserId,
        CreatedAtUtc = session.CreatedAtUtc,
        LastSeenAtUtc = session.LastSeenAtUtc,
        RevokedAtUtc = session.RevokedAtUtc,
        UserAgent = session.UserAgent,
        IpAddress = session.IpAddress,
    };
}
