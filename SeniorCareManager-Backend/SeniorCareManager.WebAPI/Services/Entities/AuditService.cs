using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SeniorCareManager.WebAPI.Data;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class AuditService : IAuditService
{
    // Nomes, não números — quem for ler BeforeValue/AfterValue depois (§8.2) precisa
    // entender "ACTIVE"/"INACTIVE" sem consultar o enum no código-fonte.
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly AppDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RecordAsync(
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
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            Id = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            Category = category,
            ActorUserId = actorUserId,
            InstitutionId = institutionId,
            TargetUserId = targetUserId,
            Resource = resource,
            Action = action,
            Feature = feature,
            TargetScopeType = targetScopeType,
            TargetScopeKey = targetScopeKey,
            Outcome = outcome,
            DeterminingLayer = determiningLayer,
            BeforeValue = beforeValue == null ? null : JsonSerializer.Serialize(beforeValue, SnapshotJsonOptions),
            AfterValue = afterValue == null ? null : JsonSerializer.Serialize(afterValue, SnapshotJsonOptions),
            // Mesmo identificador que ProblemDetails.Extensions["correlationId"] já expõe em
            // respostas de erro (Startup.cs) — sem HttpContext (ex.: chamada fora de uma
            // requisição HTTP), gera um novo para não deixar a coluna vazia.
            CorrelationId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString(),
            Description = description,
        };

        _dbContext.AuditEvents.Add(auditEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
