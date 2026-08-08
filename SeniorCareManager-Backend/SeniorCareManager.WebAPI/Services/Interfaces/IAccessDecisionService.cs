using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IAccessDecisionService
{
    // Sem cache: cada chamada lê o estado atual do banco (5.11 — nenhuma mudança em
    // papel/grupo/vínculo/exceção/política fica "presa" atrás de um contexto velho).
    Task<AccessDecision> EvaluateAsync(
        Guid actingUserId,
        string resource,
        string action,
        string? feature = null,
        AccessScopeType? targetScopeType = null,
        string? targetScopeKey = null,
        CancellationToken cancellationToken = default);

    // Exceções permanentes (ValidTo == null) exigem justificativa não-trivial (5.3).
    void ValidateOverrideJustification(string? justification, DateTime? validTo);
}
