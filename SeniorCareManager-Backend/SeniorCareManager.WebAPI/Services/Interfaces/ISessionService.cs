using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface ISessionService
{
    // Cria a sessão no login: gera a chave opaca inicial, persiste só o hash. Devolve a
    // chave em claro (vai pro cookie) — nunca é salva em claro em lugar nenhum.
    Task<(Guid SessionId, string RawKey)> CreateAsync(
        Guid userId, string? userAgent, string? ipAddress, CancellationToken cancellationToken = default);

    // Chamado a cada requisição autenticada (Events.OnValidatePrincipal). Rotaciona quando
    // passou o intervalo de acesso curto; detecta reuso (chave já rotacionada sendo
    // reapresentada) e revoga a sessão inteira nesse caso.
    Task<SessionValidationResult> ValidateAndRotateAsync(
        Guid sessionId, string presentedRawKey, CancellationToken cancellationToken = default);

    Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
