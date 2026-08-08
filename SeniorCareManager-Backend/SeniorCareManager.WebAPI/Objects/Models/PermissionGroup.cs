using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Também global/sistema (agrupa Permission por módulo) — sem seed de dados nesta
    // mudança; a §6 cria grupos via API administrativa.
    [Table("permissiongroup")]
    public class PermissionGroup
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}
