using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

// A maioria das ações fica [AllowAnonymous] individualmente (a classe não pode ter esse
// atributo — GetMe e as ações de MFA/logout exigem autenticação, e [AllowAnonymous] em
// qualquer nível sempre vence sobre [Authorize]).
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private static readonly TimeSpan MfaChallengeValidity = TimeSpan.FromMinutes(10);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountTokenService _accountTokenService;
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ISessionService _sessionService;
    private readonly IMfaPolicyService _mfaPolicyService;
    private readonly IOriginRateLimiter _originRateLimiter;
    private readonly IAuditService _auditService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IAccountTokenService accountTokenService,
        AppDbContext dbContext,
        IAccessDecisionService accessDecisionService,
        ICurrentUserContext currentUserContext,
        ISessionService sessionService,
        IMfaPolicyService mfaPolicyService,
        IOriginRateLimiter originRateLimiter,
        IAuditService auditService)
    {
        _userManager = userManager;
        _accountTokenService = accountTokenService;
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
        _currentUserContext = currentUserContext;
        _sessionService = sessionService;
        _mfaPolicyService = mfaPolicyService;
        _originRateLimiter = originRateLimiter;
        _auditService = auditService;
    }

    // Sem [RequirePermission]: ver o próprio contexto não é gated por uma permissão
    // específica (senão seria preciso já ter uma permissão para descobrir quais se tem) —
    // só exige autenticação, garantida pelo AuthorizeFilter global (§5).
    [HttpGet("me")]
    public async Task<ActionResult<CurrentIdentityDTO>> GetMe()
    {
        return Ok(await BuildCurrentIdentityAsync(_currentUserContext.UserId));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var origin = GetClientOrigin();
        if (_originRateLimiter.IsBlocked(origin))
            return TooManyRequests();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.FAILURE,
                description: $"E-mail desconhecido: {request.Email}");
            return InvalidCredentials();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            // Mesma resposta genérica das demais falhas (§4/§7.8) — um 429 distinto aqui
            // seria um oráculo de enumeração: bastaria tentar senhas erradas o suficiente
            // pra descobrir quais e-mails existem e estão bloqueados. O bloqueio real
            // continua valendo no servidor, só não é sinalizado ao cliente.
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Conta bloqueada.");
            return InvalidCredentials();
        }

        // Estado diferente de ACTIVE nunca é revelado ao cliente anônimo (§4) — mesma
        // resposta genérica de credencial inválida.
        if (user.AccountState != AccountState.ACTIVE)
        {
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: $"Estado: {user.AccountState}.");
            return InvalidCredentials();
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await RegisterFailedAttemptAsync(user);
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Senha incorreta.");
            return InvalidCredentials();
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var mfaRequired = await _mfaPolicyService.IsMfaRequiredAsync(user.Id);
        if (mfaRequired && !user.TwoFactorEnabled)
        {
            // Restringe estritamente ao fluxo de cadastro (§7.7) — nenhuma sessão chega a
            // existir até o cadastro do segundo fator terminar.
            var enrollToken = await _accountTokenService.IssueAsync(user.Id, AccountTokenPurpose.MFA_ENROLLMENT, MfaChallengeValidity);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.SUCCESS,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Cadastro de MFA obrigatório antes da sessão.");
            return Ok(new LoginResponse { Status = "mfa_enrollment_required", ChallengeToken = enrollToken });
        }

        if (user.TwoFactorEnabled)
        {
            var verifyToken = await _accountTokenService.IssueAsync(user.Id, AccountTokenPurpose.MFA_VERIFY, MfaChallengeValidity);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.SUCCESS,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Segundo fator exigido antes da sessão.");
            return Ok(new LoginResponse { Status = "mfa_required", ChallengeToken = verifyToken });
        }

        var identity = await CompleteLoginAsync(user);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Login", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);
        return Ok(new LoginResponse { Status = "ok", Identity = identity });
    }

    [HttpPost("login/mfa")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> LoginMfa(LoginMfaRequest request)
    {
        var origin = GetClientOrigin();
        if (_originRateLimiter.IsBlocked(origin))
            return TooManyRequests();

        var userId = await _accountTokenService.ValidateAsync(AccountTokenPurpose.MFA_VERIFY, request.ChallengeToken);
        if (userId == null)
        {
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaVerification", AuditOutcome.FAILURE,
                description: "Token de desafio inválido ou expirado.");
            return InvalidCredentials();
        }

        var user = await _userManager.FindByIdAsync(userId.Value.ToString());
        if (user == null || user.AccountState != AccountState.ACTIVE)
        {
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaVerification", AuditOutcome.FAILURE,
                actorUserId: userId, targetUserId: userId, description: "Conta inválida para o desafio apresentado.");
            return InvalidCredentials();
        }

        var (verified, usedRecoveryCode) = await VerifyMfaCodeAsync(user, request.Code);
        if (!verified)
        {
            _originRateLimiter.RecordFailure(origin);
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaVerification", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Código inválido.");
            // Não consome o desafio numa tentativa errada — só na que efetivamente completa
            // o login, senão um código digitado errado obrigaria a refazer o login inteiro.
            return InvalidCredentials();
        }

        await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.MFA_VERIFY, request.ChallengeToken);
        var identity = await CompleteLoginAsync(user);
        int? remainingRecoveryCodes = usedRecoveryCode ? await _userManager.CountRecoveryCodesAsync(user) : null;
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaVerification", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId,
            description: usedRecoveryCode ? "Via código de recuperação." : "Via TOTP.");
        return Ok(new LoginResponse { Status = "ok", Identity = identity, RemainingRecoveryCodes = remainingRecoveryCodes });
    }

    [HttpPost("mfa/enroll")]
    [AllowAnonymous]
    public async Task<ActionResult<MfaEnrollResponse>> MfaEnroll(MfaEnrollRequest request)
    {
        var user = await ResolveMfaTargetUserAsync(request.ChallengeToken, AccountTokenPurpose.MFA_ENROLLMENT);

        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(key))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            key = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var otpAuthUri = $"otpauth://totp/SeniorCare:{Uri.EscapeDataString(user.Email ?? user.Id.ToString())}" +
            $"?secret={key}&issuer=SeniorCare&digits=6";

        return Ok(new MfaEnrollResponse { AuthenticatorKey = key!, OtpAuthUri = otpAuthUri });
    }

    [HttpPost("mfa/confirm")]
    [AllowAnonymous]
    public async Task<ActionResult<MfaConfirmResponse>> MfaConfirm(MfaConfirmRequest request)
    {
        var user = await ResolveMfaTargetUserAsync(request.ChallengeToken, AccountTokenPurpose.MFA_ENROLLMENT);

        var codeValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.Code);
        if (!codeValid)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaEnrollmentConfirm", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Código inválido.");
            throw new BusinessRuleException("Código inválido.");
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "MfaEnrollmentConfirm", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);

        CurrentIdentityDTO? identity = null;
        if (!string.IsNullOrEmpty(request.ChallengeToken))
        {
            // Veio de um login pendente (cadastro obrigatório, §7.7) — completa a sessão.
            await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.MFA_ENROLLMENT, request.ChallengeToken);
            identity = await CompleteLoginAsync(user);
        }

        return Ok(new MfaConfirmResponse { RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>(), Identity = identity });
    }

    [HttpPost("mfa/recovery-codes/regenerate")]
    public async Task<ActionResult<MfaConfirmResponse>> RegenerateRecoveryCodes(RegenerateRecoveryCodesRequest request)
    {
        var user = await _userManager.FindByIdAsync(_currentUserContext.UserId.ToString())
            ?? throw new BusinessRuleException("Identidade autenticada inválida.");
        if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
            throw new BusinessRuleException("Senha atual inválida.");
        if (!user.TwoFactorEnabled)
            throw new BusinessRuleException("MFA não está ativo para esta conta.");

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "RecoveryCodesRegenerated", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);
        return Ok(new MfaConfirmResponse { RecoveryCodes = recoveryCodes?.ToList() ?? new List<string>() });
    }

    [HttpPost("logout")]
    public async Task<ActionResult<MessageResponse>> Logout()
    {
        var sessionIdClaim = User.FindFirst(SeniorCareClaimTypes.SessionId)?.Value;
        if (Guid.TryParse(sessionIdClaim, out var sessionId))
            await _sessionService.RevokeAsync(sessionId);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Logout", AuditOutcome.SUCCESS,
            actorUserId: _currentUserContext.UserId, institutionId: await _currentUserContext.GetInstitutionIdAsync());
        return Ok(new MessageResponse { Message = "Sessão encerrada." });
    }

    [HttpPost("activate")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> Activate(ActivateAccountRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.PROVISIONED)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Activate", AuditOutcome.FAILURE,
                actorUserId: user?.Id, targetUserId: user?.Id, institutionId: user?.InstitutionId, description: $"E-mail: {request.Email}.");
            throw new BusinessRuleException("Token ou dados de ativação inválidos.");
        }

        var tokenValid = await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.ACTIVATION, request.Token);
        if (!tokenValid)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Activate", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Token inválido ou expirado.");
            throw new BusinessRuleException("Token ou dados de ativação inválidos.");
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Activate", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Senha não atende à política.");
            throw new BusinessRuleException(DescribeErrors(addPasswordResult));
        }

        user.AccountState = AccountState.ACTIVE;
        await _userManager.UpdateAsync(user);
        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "Activate", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId,
            beforeValue: new { AccountState = AccountState.PROVISIONED }, afterValue: new { AccountState = AccountState.ACTIVE });

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
        {
            await _accountTokenService.IssueAsync(user.Id, AccountTokenPurpose.RECOVERY, AccountTokenService.RecoveryTokenValidity);
            // Só quando um token de fato é emitido — e-mail inexistente/inelegível não gera
            // evento (nada realmente aconteceu no servidor, e evitaria ruído de sondagem).
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "RecoveryRequested", AuditOutcome.SUCCESS,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);
        }

        return Ok(new MessageResponse { Message = "Se o e-mail informado tiver uma conta elegível, instruções de recuperação foram enviadas." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.ACTIVE)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordReset", AuditOutcome.FAILURE,
                actorUserId: user?.Id, targetUserId: user?.Id, institutionId: user?.InstitutionId, description: $"E-mail: {request.Email}.");
            throw new BusinessRuleException("Token ou dados de recuperação inválidos.");
        }

        var tokenValid = await _accountTokenService.ConsumeAsync(user.Id, AccountTokenPurpose.RECOVERY, request.Token);
        if (!tokenValid)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordReset", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Token inválido ou expirado.");
            throw new BusinessRuleException("Token ou dados de recuperação inválidos.");
        }

        await _userManager.RemovePasswordAsync(user);
        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordReset", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Senha não atende à política.");
            throw new BusinessRuleException(DescribeErrors(addPasswordResult));
        }

        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordReset", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);

        await _sessionService.RevokeAllForUserAsync(user.Id);
        await _auditService.RecordAsync(AuditEventCategory.SESSION, "UserSession", "RevokeAll", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Efeito colateral de redefinição de senha.");

        return Ok(new MessageResponse { Message = "Senha redefinida com sucesso." });
    }

    [HttpPost("change-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponse>> ChangePassword(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || user.AccountState != AccountState.ACTIVE)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordChanged", AuditOutcome.FAILURE,
                actorUserId: user?.Id, targetUserId: user?.Id, institutionId: user?.InstitutionId, description: $"E-mail: {request.Email}.");
            throw new BusinessRuleException("Senha atual inválida.");
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!changeResult.Succeeded)
        {
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordChanged", AuditOutcome.FAILURE,
                actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Senha atual não confere ou nova senha não atende à política.");
            throw new BusinessRuleException("Senha atual inválida.");
        }

        await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "PasswordChanged", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId);

        await _sessionService.RevokeAllForUserAsync(user.Id);
        await _auditService.RecordAsync(AuditEventCategory.SESSION, "UserSession", "RevokeAll", AuditOutcome.SUCCESS,
            actorUserId: user.Id, targetUserId: user.Id, institutionId: user.InstitutionId, description: "Efeito colateral de troca de senha.");

        return Ok(new MessageResponse { Message = "Senha alterada com sucesso." });
    }

    private async Task<(bool Verified, bool UsedRecoveryCode)> VerifyMfaCodeAsync(ApplicationUser user, string code)
    {
        if (await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code))
            return (true, false);

        var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
        return (recoveryResult.Succeeded, recoveryResult.Succeeded);
    }

    private async Task<ApplicationUser> ResolveMfaTargetUserAsync(string? challengeToken, AccountTokenPurpose purpose)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await _userManager.FindByIdAsync(_currentUserContext.UserId.ToString())
                ?? throw new BusinessRuleException("Identidade autenticada inválida.");
        }

        if (string.IsNullOrEmpty(challengeToken))
            throw new BusinessRuleException("Autenticação ou token de desafio exigido.");

        var userId = await _accountTokenService.ValidateAsync(purpose, challengeToken);
        if (userId == null)
            throw new BusinessRuleException("Token de desafio inválido ou expirado.");

        return await _userManager.FindByIdAsync(userId.Value.ToString())
            ?? throw new BusinessRuleException("Token de desafio inválido ou expirado.");
    }

    private async Task RegisterFailedAttemptAsync(ApplicationUser user)
    {
        await _userManager.AccessFailedAsync(user);

        var institution = await _dbContext.Institutions.FindAsync(user.InstitutionId);
        var maxAttempts = institution?.MaxFailedAttempts ?? InstitutionSecurityPolicyService.DefaultMaxFailedAttempts;
        var lockoutMinutes = institution?.LockoutDurationMinutes ?? InstitutionSecurityPolicyService.DefaultLockoutDurationMinutes;

        var refreshed = await _userManager.FindByIdAsync(user.Id.ToString());
        if (refreshed != null && refreshed.AccessFailedCount >= maxAttempts)
        {
            await _userManager.SetLockoutEndDateAsync(refreshed, DateTimeOffset.UtcNow.AddMinutes(lockoutMinutes));
            await _auditService.RecordAsync(AuditEventCategory.AUTHENTICATION, "Auth", "AccountLockout", AuditOutcome.SUCCESS,
                targetUserId: refreshed.Id, institutionId: refreshed.InstitutionId,
                description: $"{refreshed.AccessFailedCount} tentativas malsucedidas, bloqueio de {lockoutMinutes} min.");
        }
    }

    private async Task<CurrentIdentityDTO> CompleteLoginAsync(ApplicationUser user)
    {
        var (sessionId, rawKey) = await _sessionService.CreateAsync(
            user.Id, Request.Headers.UserAgent.ToString(), GetClientOrigin());

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        identity.AddClaim(new Claim(SeniorCareClaimTypes.InstitutionId, user.InstitutionId.ToString()));
        identity.AddClaim(new Claim(SeniorCareClaimTypes.SessionId, sessionId.ToString()));
        identity.AddClaim(new Claim(SeniorCareClaimTypes.SessionKey, rawKey));

        var session = await _dbContext.UserSessions.SingleAsync(s => s.Id == sessionId);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = session.ExpiresAtUtc });

        return await BuildCurrentIdentityAsync(user.Id);
    }

    private async Task<CurrentIdentityDTO> BuildCurrentIdentityAsync(Guid userId)
    {
        var user = await _dbContext.Users.SingleAsync(u => u.Id == userId);
        var institution = await _dbContext.Institutions.SingleAsync(i => i.Id == user.InstitutionId);

        var roleNames = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        var now = DateTime.UtcNow;
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

        return new CurrentIdentityDTO
        {
            UserId = user.Id,
            InstitutionId = institution.Id,
            InstitutionName = institution.Name,
            DisplayName = user.DisplayName,
            Email = user.Email ?? string.Empty,
            Roles = roleNames,
            OrganizationalResponsibilities = responsibilities,
            EffectivePermissions = effectivePermissions,
        };
    }

    private string GetClientOrigin() => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private ObjectResult TooManyRequests() =>
        StatusCode(StatusCodes.Status429TooManyRequests, new MessageResponse { Message = "Muitas tentativas. Tente novamente mais tarde." });

    private UnauthorizedObjectResult InvalidCredentials() =>
        Unauthorized(new MessageResponse { Message = "Credenciais inválidas." });

    private static string DescribeErrors(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(e => e.Description));
}
