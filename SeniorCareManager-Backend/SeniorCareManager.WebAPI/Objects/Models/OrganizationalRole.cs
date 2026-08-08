using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Responsabilidade organizacional (distinta do papel técnico Role) — também composta
    // de PermissionGroup, mas só concede capacidade através de um OrganizationalRoleAssignment
    // válido (com escopo e período de validade).
    [Table("organizationalrole")]
    public class OrganizationalRole
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("institution_id")]
        public Guid InstitutionId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}
