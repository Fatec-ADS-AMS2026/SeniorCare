using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IAccountTokenService
{
    // Gera um token aleatório, curto e de uso único; persiste apenas o hash e devolve o
    // valor bruto (que nunca é salvo) para ser entregue fora de banda ao usuário.
    Task<string> IssueAsync(Guid userId, AccountTokenPurpose purpose, TimeSpan validity, CancellationToken cancellationToken = default);

    // Retorna true e marca o token como usado se ele for válido (existe, não expirou, não
    // foi usado, pertence ao usuário e propósito informados); caso contrário, false.
    Task<bool> ConsumeAsync(Guid userId, AccountTokenPurpose purpose, string rawToken, CancellationToken cancellationToken = default);

    // Só confere validade (existe, não expirou, não foi usado) e resolve o UserId dono do
    // token — não marca como usado. Necessário no login por desafio (§7): o cliente só tem o
    // token, não sabe o UserId, e uma tentativa de código errada não pode queimar o desafio.
    Task<Guid?> ValidateAsync(AccountTokenPurpose purpose, string rawToken, CancellationToken cancellationToken = default);
}
