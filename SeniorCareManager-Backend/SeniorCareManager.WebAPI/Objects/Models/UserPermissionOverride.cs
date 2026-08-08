using System;
using System.ComponentModel.DataAnnotations.Schema;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Exceção individual ALLOW/DENY. ValidTo == null (permanente) exige Justification
    // não-vazia — validado em serviço, não em constraint de banco.
    [Table("userpermissionoverride")]
    public class UserPermissionOverride
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

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

        [Column("justification")]
        public string Justification { get; set; } = string.Empty;

        [Column("granted_by_user_id")]
        public Guid GrantedByUserId { get; set; }

        [Column("valid_from")]
        public DateTime ValidFrom { get; set; }

        [Column("valid_to")]
        public DateTime? ValidTo { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
