using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class InstitutionModuleProvisioningService : IInstitutionModuleProvisioningService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<InstitutionModuleProvisioningService> _logger;

    public InstitutionModuleProvisioningService(AppDbContext dbContext, ILogger<InstitutionModuleProvisioningService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        var institutionIds = await _dbContext.Institutions.Select(i => i.Id).ToListAsync(cancellationToken);
        if (institutionIds.Count == 0)
            return 0;

        var moduleDefinitions = await _dbContext.ModuleDefinitions
            .Where(md => md.IsActive)
            .OrderBy(md => md.Id)
            .ToListAsync(cancellationToken);
        if (moduleDefinitions.Count == 0)
            return 0;

        var existingPairs = (await _dbContext.InstitutionModules
                .Select(im => new { im.InstitutionId, im.ModuleDefinitionId })
                .ToListAsync(cancellationToken))
            .ToHashSet();

        var now = DateTime.UtcNow;
        var created = 0;

        foreach (var institutionId in institutionIds)
        {
            for (var i = 0; i < moduleDefinitions.Count; i++)
            {
                var moduleDefinition = moduleDefinitions[i];
                var pair = new { InstitutionId = institutionId, ModuleDefinitionId = moduleDefinition.Id };
                if (existingPairs.Contains(pair))
                    continue;

                _dbContext.InstitutionModules.Add(new InstitutionModule
                {
                    InstitutionId = institutionId,
                    ModuleDefinitionId = moduleDefinition.Id,
                    OperationalState = OperationalState.DISABLED,
                    IsEnabled = false,
                    Order = i + 1,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                });
                created++;
            }
        }

        if (created > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }
}
