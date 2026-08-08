using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("organizationalrolepermissiongroup")]
    public class OrganizationalRolePermissionGroup
    {
        [Column("organizational_role_id")]
        public Guid OrganizationalRoleId { get; set; }

        [Column("permission_group_id")]
        public Guid PermissionGroupId { get; set; }
    }
}
