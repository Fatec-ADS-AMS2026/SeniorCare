using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Os 4 endpoints de credencial ficam [AllowAnonymous] individualmente (sessão/login só
// chega na §7) — GetMe (§6.8) é o primeiro endpoint autenticado deste controller, então a
// classe não pode ter [AllowAnonymous] (esse atributo, em qualquer nível, sempre vence sobre
// [Authorize], então precisaria estar só nas 4 ações que devem continuar públicas).
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountTokenService _accountTokenService;
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly ICurrentUserContext _currentUserContext;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IAccountTokenService accountTokenService,
        AppDbContext dbContext,
        IAccessDecisionService accessDecisionService,
        ICurrentUserContext currentUserContext)
    {
        _userManager = userManager;
        _accountTokenService = accountTokenService;
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
        _currentUserContext = currentUserContext;
    }

    // Sem [RequirePermission]: ver o próprio contexto não é gated por uma permissão
    // específica (senão seria preciso já ter uma permissão para descobrir quais se tem) —
    // só exige autenticação, garantida pelo AuthorizeFilter global (§5).
    [HttpGet("me")]
    public async Task<ActionResult<CurrentIdentityDTO>> GetMe()
    {
        var userId = _currentUserContext.UserId;
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);
        var institution = await _dbContext.Institutions.SingleAsync(i => i.Id == user.InstitutionId);

        var roleNames = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        var now = System.DateTime.UtcNow;
        var responsibilities = await _dbContext.OrganizationalRoleAssignments
            .Where(a => a.UserId == userId && a.ValidFrom <= now && (a.ValidTo == null || a.ValidTo >= now))
            .Join(_dbContext.OrganizationalRoles, a => a.OrganizationalRoleId, r => r.Id,
                (a, r) => new OrganizationalResponsibilityDTO { Name = r.Name, ScopeType = a.ScopeType.ToString(), ScopeKey = a.ScopeKey })
            .ToListAsync();

        var allPermissions = await _dbContext.Permissions.ToListAsync();
        var effectivePermissions = new List<EffectivePermissionDTO>();
        foreach (var permission in allPermissions)
        {
            var decision = await _accessDecisionService.EvaluateAsync(userId, permission.Resource, permission.Action, permission.Feature);
            if (decision.Allowed)
                effectivePermissions.Add(new EffectivePermissionDTO { Resource = permission.Resource, Action = permission.Action, Feature = permission.Feature });
        }

        return Ok(new CurrentIdentityDTO
        {
            UserId = user.Id,
            InstitutionId = institution.Id,
            InstitutionName = institution.Name,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Roles = roleNames,
            OrganizationalResponsibilities = responsibilities,
            EffectivePermissions = effectivePermissions,
        });
    }

    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> Activate(ActivateAccountRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.PROVISIONED)
            throw new BusinessRuleException("Token ou dados de ativação inválidos.");

        var tokenValid = await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.ACTIVATION, request.Token);
        if (!tokenValid)
            throw new BusinessRuleException("Token ou dados de ativação inválidos.");

        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
            throw new BusinessRuleException(DescribeErrors(addPasswordResult));

        user.AccountState = AccountState.ACTIVE;
        await _userManager.UpdateAsync(user);

        return Ok(new MessageResponse { Message = "Conta ativada com sucesso." });
    }

    [HttpPost("recover")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> Recover(RecoverAccountRequest request)
    {
        // Resposta uniforme para impedir enumeração de contas: mesmo corpo de sucesso quer o
        // e-mail exista ou não, e quer a conta seja elegível ou não.
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null && user.AccountState == AccountState.ACTIVE)
            await _accountTokenService.IssueAsync(user.Id, AccountTokenPurpose.RECOVERY, AccountTokenService.RecoveryTokenValidity);

        return Ok(new MessageResponse { Message = "Se o e-mail informado tiver uma conta elegível, instruções de recuperação foram enviadas." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.ACTIVE)
            throw new BusinessRuleException("Token ou dados de recuperação inválidos.");

        var tokenValid = await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.RECOVERY, request.Token);
        if (!tokenValid)
            throw new BusinessRuleException("Token ou dados de recuperação inválidos.");

        await _userManager.RemovePasswordAsync(user);
        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
            throw new BusinessRuleException(DescribeErrors(addPasswordResult));

        return Ok(new MessageResponse { Message = "Senha redefinida com sucesso." });
    }

    [HttpPost("change-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ChangePassword(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.ACTIVE)
            throw new BusinessRuleException("Senha atual inválida.");

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
            throw new BusinessRuleException("Senha atual inválida.");

        return Ok(new MessageResponse { Message = "Senha alterada com sucesso." });
    }

    private static string DescribeErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
