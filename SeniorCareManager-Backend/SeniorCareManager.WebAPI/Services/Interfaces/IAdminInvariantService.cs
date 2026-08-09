using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IAdminInvariantService
{
    // Lança BusinessRuleException se `targetUserId` for o último administrador institucional
    // ativo (única conta ACTIVE com a permissão marcadora AccessAdministration/manage) —
    // impede tirar essa conta de ACTIVE e deixar a instituição sem ninguém que administre
    // acesso (§6.7). No-op se o alvo não for administrador.
    Task EnsureNotLastActiveAdministratorAsync(Guid institutionId, Guid targetUserId, CancellationToken cancellationToken = default);
}
