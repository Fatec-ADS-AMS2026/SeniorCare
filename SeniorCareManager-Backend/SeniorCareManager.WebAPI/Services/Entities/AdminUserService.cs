using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountTokenService _accountTokenService;

    public AdminUserService(UserManager<ApplicationUser> userManager, IAccountTokenService accountTokenService)
    {
        _userManager = userManager;
        _accountTokenService = accountTokenService;
    }

    public async Task<(Guid UserId, string ActivationToken)> CreateAsync(
        Guid institutionId, string email, string displayName, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            InstitutionId = institutionId,
            DisplayName = displayName,
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.PROVISIONED
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new BusinessRuleException($"Não foi possível criar a conta: {errors}");
        }

        var activationToken = await _accountTokenService.IssueAsync(
            user.Id, AccountTokenPurpose.ACTIVATION, AccountTokenService.ActivationTokenValidity, cancellationToken);

        return (user.Id, activationToken);
    }
}
