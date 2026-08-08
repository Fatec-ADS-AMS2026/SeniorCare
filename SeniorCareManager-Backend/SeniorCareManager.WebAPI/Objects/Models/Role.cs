using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Papel técnico por instituição — composto de PermissionGroup (RolePermissionGroup),
    // atribuído a usuários diretamente via UserRole. Distinto de OrganizationalRole
    // (responsabilidade organizacional, com escopo e validade).
    [Table("role")]
    public class Role
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("institution_id")]
        public Guid InstitutionId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}
