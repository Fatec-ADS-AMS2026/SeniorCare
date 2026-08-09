using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeniorCareManager.WebAPI.Infrastructure;

// Resolve (UserId, InstitutionId) uma vez a partir do ClaimsPrincipal + banco — usado pelos
// controllers administrativos para escopar toda consulta/mutação à instituição de quem está
// autenticado (isolamento institucional). Distinto da resolução de usuário já embutida em
// RequirePermissionAttribute, que serve só à decisão de autorização.
public interface ICurrentUserContext
{
    Guid UserId { get; }

    Task<Guid> GetInstitutionIdAsync(CancellationToken cancellationToken = default);
}
