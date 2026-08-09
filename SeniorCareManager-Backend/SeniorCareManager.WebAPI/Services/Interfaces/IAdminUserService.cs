using System;
using System.Threading;
using System.Threading.Tasks;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IAdminUserService
{
    // Cria uma conta PROVISIONED na instituição informada e emite o token de ativação —
    // mesmo princípio do bootstrap (§4): a operação nunca conhece nem transmite senha.
    Task<(Guid UserId, string ActivationToken)> CreateAsync(
        Guid institutionId, string email, string displayName, CancellationToken cancellationToken = default);
}
