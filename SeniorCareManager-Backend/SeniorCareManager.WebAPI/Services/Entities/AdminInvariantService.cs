using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class AdminInvariantService : IAdminInvariantService
{
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;

    public AdminInvariantService(AppDbContext dbContext, IAccessDecisionService accessDecisionService)
    {
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
    }

    public async Task EnsureNotLastActiveAdministratorAsync(Guid institutionId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var targetIsAdministrator = (await _accessDecisionService.EvaluateAsync(
            targetUserId, "AccessAdministration", "manage", cancellationToken: cancellationToken)).Allowed;
        if (!targetIsAdministrator)
            return;

        var otherActiveUserIds = await _dbContext.Users
            .Where(u => u.InstitutionId == institutionId && u.AccountState == AccountState.ACTIVE && u.Id != targetUserId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        foreach (var userId in otherActiveUserIds)
        {
            var decision = await _accessDecisionService.EvaluateAsync(
                userId, "AccessAdministration", "manage", cancellationToken: cancellationToken);
            if (decision.Allowed)
                return;
        }

        throw new BusinessRuleException("Não é possível remover o último administrador institucional ativo.");
    }
}
