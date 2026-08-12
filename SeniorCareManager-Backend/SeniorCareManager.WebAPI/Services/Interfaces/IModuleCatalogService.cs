using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IModuleCatalogService
{
    // introduce-senior-portal §3.1/§3.2 — catálogo mínimo do usuário autenticado no
    // contexto atual (instituição + conta + permissões efetivas), ordenado
    // deterministicamente e sem módulos desabilitados ou não autorizados.
    Task<IReadOnlyList<ModuleCatalogItemDTO>> GetForCurrentUserAsync(CancellationToken cancellationToken = default);
}
