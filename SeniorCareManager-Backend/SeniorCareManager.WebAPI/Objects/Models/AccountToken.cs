using System;
using System.ComponentModel.DataAnnotations.Schema;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("accounttoken")]
    public class AccountToken
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("purpose")]
        public AccountTokenPurpose Purpose { get; set; }

        // SHA-256 do token aleatório apresentado ao usuário — o token em si nunca é
        // persistido, só o hash (spec: "armazenado por hash").
        [Column("token_hash")]
        public string TokenHash { get; set; } = string.Empty;

        [Column("expires_at_utc")]
        public DateTime ExpiresAtUtc { get; set; }

        [Column("used_at_utc")]
        public DateTime? UsedAtUtc { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }
    }
}
