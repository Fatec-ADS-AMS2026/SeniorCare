using System;
using System.ComponentModel.DataAnnotations.Schema;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Política condicional institucional. Editar cria uma NOVA linha (mesma PolicyKey,
    // Version+1) e aposenta a anterior (State=RETIRED) — histórico real, não campo mutável.
    // Condições são colunas tipadas (vocabulário fechado), não uma expressão livre.
    [Table("accesspolicy")]
    public class AccessPolicy
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("policy_key")]
        public Guid PolicyKey { get; set; }

        [Column("version")]
        public int Version { get; set; }

        [Column("institution_id")]
        public Guid InstitutionId { get; set; }

        [Column("resource")]
        public string Resource { get; set; } = string.Empty;

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("feature")]
        public string? Feature { get; set; }

        [Column("scope_type")]
        public AccessScopeType? ScopeType { get; set; }

        [Column("scope_key")]
        public string? ScopeKey { get; set; }

        [Column("effect")]
        public AccessEffect Effect { get; set; }

        [Column("state")]
        public AccessPolicyState State { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("created_by_user_id")]
        public Guid CreatedByUserId { get; set; }
    }
}
