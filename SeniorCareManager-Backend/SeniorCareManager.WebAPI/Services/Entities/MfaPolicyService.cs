using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class MfaPolicyService : IMfaPolicyService
{
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;

    public MfaPolicyService(AppDbContext dbContext, IAccessDecisionService accessDecisionService)
    {
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
    }

    public async Task<bool> IsMfaRequiredAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
            return false;

        if (user.IsSystemAdmin)
            return true;

        var isAccessAdministrator = (await _accessDecisionService.EvaluateAsync(
            userId, "AccessAdministration", "manage", cancellationToken: cancellationToken)).Allowed;
        if (isAccessAdministrator)
            return true;

        var institution = await _dbContext.Institutions.FindAsync(new object[] { user.InstitutionId }, cancellationToken);
        return institution?.MfaRequiredForAllUsers ?? false;
    }
}
