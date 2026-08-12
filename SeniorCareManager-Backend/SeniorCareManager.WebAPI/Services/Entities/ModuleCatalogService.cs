using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

// introduce-senior-portal §3.1 — combina instituição atual (ICurrentUserContext), conta
// ativa e permissões efetivas (IAccessDecisionService, mesma precedência usada em todo o
// resto do sistema — nunca reimplementada aqui) com a configuração institucional do
// catálogo (InstitutionModule + ModuleDefinition) pra produzir a resposta mínima de
// GET /api/v1/me/modules (design.md decisão 5).
public class ModuleCatalogService : IModuleCatalogService
{
    private readonly AppDbContext _dbContext;
    private readonly IAccessDecisionService _accessDecisionService;
    private readonly ICurrentUserContext _currentUserContext;

    public ModuleCatalogService(
        AppDbContext dbContext, IAccessDecisionService accessDecisionService, ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _accessDecisionService = accessDecisionService;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<ModuleCatalogItemDTO>> GetForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var institutionId = await _currentUserContext.GetInstitutionIdAsync(cancellationToken);

        // §3.2 — "desabilitado" cobre as duas formas de ficar de fora do catálogo
        // efetivo: a instituição nunca habilitou o módulo (IsEnabled=false, estado
        // inicial do provisionamento em §2.3) ou um admin o desligou depois
        // (OperationalState=DISABLED, spec.md "estados operacionais"). A definição
        // sistêmica também precisa estar ativa — provisionamento nunca cria linha pra
        // uma definição inativa, mas uma definição pode ser desativada depois de já
        // provisionada.
        var candidates = await _dbContext.InstitutionModules
            .Where(im => im.InstitutionId == institutionId
                && im.IsEnabled
                && im.OperationalState != OperationalState.DISABLED)
            .Include(im => im.ModuleDefinition)
            .ThenInclude(md => md!.RequiredPermission)
            .Where(im => im.ModuleDefinition!.IsActive)
            .OrderBy(im => im.Order)
            .ThenBy(im => im.ModuleDefinition!.Key)
            .ToListAsync(cancellationToken);

        var result = new List<ModuleCatalogItemDTO>(candidates.Count);
        foreach (var institutionModule in candidates)
        {
            var permission = institutionModule.ModuleDefinition!.RequiredPermission!;
            var decision = await _accessDecisionService.EvaluateAsync(
                _currentUserContext.UserId, permission.Resource, permission.Action, cancellationToken: cancellationToken);
            if (!decision.Allowed)
                continue;

            result.Add(ToDto(institutionModule));
        }

        return result;
    }

    private static ModuleCatalogItemDTO ToDto(InstitutionModule institutionModule)
    {
        var definition = institutionModule.ModuleDefinition!;
        return new ModuleCatalogItemDTO
        {
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            Icon = definition.Icon,
            Path = definition.Path,
            Order = institutionModule.Order,
            OperationalState = institutionModule.OperationalState,
            OperationalMessage = institutionModule.OperationalMessage,
        };
    }
}
