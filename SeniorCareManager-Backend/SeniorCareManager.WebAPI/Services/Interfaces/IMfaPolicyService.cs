using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IMfaPolicyService
{
    // Administradores e contas de configuração de acesso (§6: IsSystemAdmin ou permissão
    // marcadora AccessAdministration/manage) sempre exigem MFA; os demais só se a
    // instituição exigir (Institution.MfaRequiredForAllUsers).
    Task<bool> IsMfaRequiredAsync(Guid userId, CancellationToken cancellationToken = default);
}
