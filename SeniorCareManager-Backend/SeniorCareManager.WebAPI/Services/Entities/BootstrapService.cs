using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class BootstrapService : IBootstrapService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAccountTokenService _accountTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BootstrapService> _logger;

    public BootstrapService(
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IAccountTokenService accountTokenService,
        IConfiguration configuration,
        ILogger<BootstrapService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _accountTokenService = accountTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<BootstrapResult> RunAsync(CancellationToken cancellationToken = default)
    {
        if (await _dbContext.Institutions.AnyAsync(cancellationToken))
            return new BootstrapResult { Created = false };

        var institutionName = _configuration["Bootstrap:InstitutionName"];
        var adminEmail = _configuration["Bootstrap:AdminEmail"];
        var adminDisplayName = _configuration["Bootstrap:AdminDisplayName"];

        if (string.IsNullOrWhiteSpace(institutionName) || string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminDisplayName))
        {
            _logger.LogWarning(
                "Bootstrap não executado: nenhuma instituição existe ainda e as variáveis " +
                "Bootstrap__InstitutionName/Bootstrap__AdminEmail/Bootstrap__AdminDisplayName não foram todas informadas.");
            return new BootstrapResult { Created = false };
        }

        var institution = new Institution(Guid.NewGuid(), institutionName);
        _dbContext.Institutions.Add(institution);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            InstitutionId = institution.Id,
            DisplayName = adminDisplayName,
            IdentityOrigin = IdentityOrigin.LOCAL,
            AccountState = AccountState.PROVISIONED
        };

        // Sem senha: a conta PROVISIONED só ganha credencial pelo fluxo de ativação por
        // token — a operação nunca conhece nem transmite uma senha do administrador.
        var createResult = await _userManager.CreateAsync(admin);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Falha ao criar a conta administrativa de bootstrap: {errors}");
        }

        var activationToken = await _accountTokenService.IssueAsync(
            admin.Id, AccountTokenPurpose.ACTIVATION, AccountTokenService.ActivationTokenValidity, cancellationToken);

        return new BootstrapResult
        {
            Created = true,
            AdminEmail = adminEmail,
            ActivationToken = activationToken
        };
    }
}
