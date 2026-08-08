using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("rolepermissiongroup")]
    public class RolePermissionGroup
    {
        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("permission_group_id")]
        public Guid PermissionGroupId { get; set; }
    }
}
