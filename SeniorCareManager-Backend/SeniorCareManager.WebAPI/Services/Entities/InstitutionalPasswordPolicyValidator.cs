using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

// Substitui as regras de composição padrão do Identity (desligadas em Startup.cs) pelas
// regras da spec: piso 15 sem MFA / 8 com MFA, aceita >=64 chars/espaço/Unicode sem truncar,
// bloqueia senha comum/vazada.
public class InstitutionalPasswordPolicyValidator : IPasswordValidator<ApplicationUser>
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordPolicyService _passwordPolicyService;
    private readonly ICommonPasswordBlocklist _commonPasswordBlocklist;

    public InstitutionalPasswordPolicyValidator(
        AppDbContext dbContext,
        IPasswordPolicyService passwordPolicyService,
        ICommonPasswordBlocklist commonPasswordBlocklist)
    {
        _dbContext = dbContext;
        _passwordPolicyService = passwordPolicyService;
        _commonPasswordBlocklist = commonPasswordBlocklist;
    }

    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return IdentityResult.Failed(new IdentityError { Code = "PasswordRequired", Description = "A senha é obrigatória." });

        var institution = await _dbContext.Institutions.FindAsync(user.InstitutionId);
        var minLength = _passwordPolicyService.GetEffectiveMinLength(institution, user.TwoFactorEnabled);

        if (password.Length < minLength)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooShort",
                Description = $"A senha deve ter no mínimo {minLength} caracteres."
            });

        if (password.Length > PasswordPolicyService.MaxLength)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordTooLong",
                Description = $"A senha deve ter no máximo {PasswordPolicyService.MaxLength} caracteres."
            });

        if (_commonPasswordBlocklist.IsCommon(password))
            return IdentityResult.Failed(new IdentityError
            {
                Code = "PasswordCommon",
                Description = "Essa senha é muito comum ou já apareceu em vazamentos conhecidos. Escolha outra."
            });

        return IdentityResult.Success;
    }
}
