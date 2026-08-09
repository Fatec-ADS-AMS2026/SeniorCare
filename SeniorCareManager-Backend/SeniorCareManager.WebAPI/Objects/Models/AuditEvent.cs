using System;
using System.ComponentModel.DataAnnotations.Schema;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // §8: registro append-only de autenticação, sessão, configuração, catálogo e decisão de
    // acesso. Nunca contém senha, token, segredo ou código MFA — BeforeValue/AfterValue são
    // sempre um snapshot mínimo montado por quem chama IAuditService, nunca a entidade EF
    // inteira serializada. Imutabilidade é reforçada em dois níveis: AuditImmutabilityInterceptor
    // (EF) e uma trigger no Postgres (ver migração) — nenhum dos dois vive neste arquivo.
    [Table("audit_event")]
    public class AuditEvent
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("occurred_at_utc")]
        public DateTime OccurredAtUtc { get; set; }

        [Column("category")]
        public AuditEventCategory Category { get; set; }

        // Quem executou a ação — nulo quando não há ator autenticado (ex.: tentativa de login
        // com e-mail inexistente).
        [Column("actor_user_id")]
        public Guid? ActorUserId { get; set; }

        [Column("institution_id")]
        public Guid? InstitutionId { get; set; }

        // Conta afetada pela ação, quando distinta do ator (ex.: admin revoga sessão de outra
        // pessoa). Nulo quando ator e alvo coincidem ou não há alvo (ex.: mudança de parâmetro
        // institucional).
        [Column("target_user_id")]
        public Guid? TargetUserId { get; set; }

        [Column("resource")]
        public string Resource { get; set; } = string.Empty;

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("feature")]
        public string? Feature { get; set; }

        [Column("target_scope_type")]
        public AccessScopeType? TargetScopeType { get; set; }

        [Column("target_scope_key")]
        public string? TargetScopeKey { get; set; }

        [Column("outcome")]
        public AuditOutcome Outcome { get; set; }

        // Só preenchido em Category=ACCESS_DECISION — replica AccessDecision.DeterminingLayer
        // (AccessDecisionService), ex.: "SYSTEM_ADMIN", "RBAC", "DEFAULT_DENY".
        [Column("determining_layer")]
        public string? DeterminingLayer { get; set; }

        // JSON (jsonb) — snapshot mínimo, nunca a entidade inteira.
        [Column("before_value")]
        public string? BeforeValue { get; set; }

        [Column("after_value")]
        public string? AfterValue { get; set; }

        // HttpContext.TraceIdentifier — mesmo identificador já usado em
        // ProblemDetails.Extensions["correlationId"] (GlobalExceptionHandler/Startup).
        [Column("correlation_id")]
        public string CorrelationId { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }
    }
}
