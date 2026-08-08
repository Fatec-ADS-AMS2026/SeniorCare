using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Endpoints anônimos e autocontidos: sessão/login só chega na §7, então cada operação aqui
// recebe no próprio corpo tudo que precisa para provar a identidade (token de uso único ou
// a senha atual), sem depender de cookie/bearer.
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountTokenService _accountTokenService;

    public AuthController(UserManager<ApplicationUser> userManager, IAccountTokenService accountTokenService)
    {
        _userManager = userManager;
        _accountTokenService = accountTokenService;
    }

    [HttpPost("activate")]
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
