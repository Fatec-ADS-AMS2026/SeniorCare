using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("userrole")]
    public class UserRole
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
