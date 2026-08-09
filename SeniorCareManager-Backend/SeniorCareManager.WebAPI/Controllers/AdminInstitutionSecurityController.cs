using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Parâmetros de segurança institucionais (§6.5) — consumidos de fato pela §7
// (login/MFA/rate limit ainda não existem); a configuração já fica pronta e validada.
[ApiController]
[Route("api/v1/[controller]")]
public class AdminInstitutionSecurityController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IInstitutionSecurityPolicyService _securityPolicyService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAuditService _auditService;

    public AdminInstitutionSecurityController(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IInstitutionSecurityPolicyService securityPolicyService,
        ICurrentUserContext currentUserContext,
        IAuditService auditService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _securityPolicyService = securityPolicyService;
        _currentUserContext = currentUserContext;
        _auditService = auditService;
    }

    [HttpGet]
    [RequirePermission("InstitutionSecurityPolicy", "read")]
    public async Task<ActionResult<InstitutionSecurityPolicyDTO>> Get()
    {
        var institution = await GetInstitutionAsync();
        return Ok(ToDto(institution));
    }

    [HttpPut]
    [RequirePermission("InstitutionSecurityPolicy", "write")]
    public async Task<ActionResult<InstitutionSecurityPolicyDTO>> Put(InstitutionSecurityPolicyUpdateRequest request)
    {
        var actingUser = await _userManager.FindByIdAsync(_currentUserContext.UserId.ToString())
            ?? throw new BusinessRuleException("Identidade autenticada inválida.");
        if (!await _userManager.CheckPasswordAsync(actingUser, request.CurrentPassword))
            throw new BusinessRuleException("Senha atual inválida.");

        _securityPolicyService.ValidateSecuritySettings(
            request.LockoutDurationMinutes, request.MaxFailedAttempts,
            request.AccessTokenDurationMinutes, request.RefreshTokenDurationDays);

        var institution = await GetInstitutionAsync();
        var previousSettings = ToDto(institution);
        institution.LockoutDurationMinutes = request.LockoutDurationMinutes;
        institution.MaxFailedAttempts = request.MaxFailedAttempts;
        institution.AccessTokenDurationMinutes = request.AccessTokenDurationMinutes;
        institution.RefreshTokenDurationDays = request.RefreshTokenDurationDays;
        institution.MfaRequiredForAllUsers = request.MfaRequiredForAllUsers;
        await _dbContext.SaveChangesAsync();
        await _auditService.RecordAsync(AuditEventCategory.CONFIGURATION, "InstitutionSecurityPolicy", "Update", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: institution.Id,
            beforeValue: previousSettings, afterValue: ToDto(institution));

        return Ok(ToDto(institution));
    }

    private async Task<Institution> GetInstitutionAsync()
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync();
        return await _dbContext.Institutions.SingleAsync(i => i.Id == institutionId);
    }

    private static InstitutionSecurityPolicyDTO ToDto(Institution institution) => new()
    {
        LockoutDurationMinutes = institution.LockoutDurationMinutes,
        MaxFailedAttempts = institution.MaxFailedAttempts,
        AccessTokenDurationMinutes = institution.AccessTokenDurationMinutes,
        RefreshTokenDurationDays = institution.RefreshTokenDurationDays,
        MfaRequiredForAllUsers = institution.MfaRequiredForAllUsers,
    };
}
