using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("permissiongrouppermission")]
    public class PermissionGroupPermission
    {
        [Column("permission_group_id")]
        public Guid PermissionGroupId { get; set; }

        [Column("permission_id")]
        public Guid PermissionId { get; set; }
    }
}
