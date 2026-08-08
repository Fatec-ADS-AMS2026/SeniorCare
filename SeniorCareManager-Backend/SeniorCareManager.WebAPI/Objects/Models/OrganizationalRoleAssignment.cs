using System;
using System.ComponentModel.DataAnnotations.Schema;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("organizationalroleassignment")]
    public class OrganizationalRoleAssignment
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("organizational_role_id")]
        public Guid OrganizationalRoleId { get; set; }

        [Column("scope_type")]
        public AccessScopeType ScopeType { get; set; }

        // Sem FK — não existe catálogo de Unidade/Setor ainda; identificador livre.
        [Column("scope_key")]
        public string? ScopeKey { get; set; }

        [Column("valid_from")]
        public DateTime ValidFrom { get; set; }

        [Column("valid_to")]
        public DateTime? ValidTo { get; set; }
    }
}
