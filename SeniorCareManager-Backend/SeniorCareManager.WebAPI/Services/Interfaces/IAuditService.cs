using System;
using System.Threading;
using System.Threading.Tasks;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IAuditService
{
    // Autocontido: grava e salva por conta própria (não tenta compartilhar transação com a
    // mutação de negócio que acabou de acontecer) — troca atomicidade perfeita por
    // simplicidade em todo call site (§8, decisão registrada em design/tasks).
    // beforeValue/afterValue devem ser um snapshot mínimo (ex.: objeto anônimo só com os
    // campos relevantes) — NUNCA a entidade EF inteira, para não vazar senha/token/segredo.
    Task RecordAsync(
        AuditEventCategory category,
        string resource,
        string action,
        AuditOutcome outcome,
        Guid? actorUserId = null,
        Guid? institutionId = null,
        Guid? targetUserId = null,
        string? feature = null,
        AccessScopeType? targetScopeType = null,
        string? targetScopeKey = null,
        string? determiningLayer = null,
        object? beforeValue = null,
        object? afterValue = null,
        string? description = null,
        CancellationToken cancellationToken = default);
}
