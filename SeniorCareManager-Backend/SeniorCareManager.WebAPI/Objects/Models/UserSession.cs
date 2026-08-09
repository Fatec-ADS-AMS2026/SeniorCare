using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // §7: sessão real, com rotação e detecção de reuso sobre o mesmo cookie único
    // registrado na §5 (Events.OnValidatePrincipal) — nunca a chave em claro, só o hash.
    [Table("usersession")]
    public class UserSession
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("created_at_utc")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("last_seen_at_utc")]
        public DateTime LastSeenAtUtc { get; set; }

        [Column("revoked_at_utc")]
        public DateTime? RevokedAtUtc { get; set; }

        [Column("user_agent")]
        public string? UserAgent { get; set; }

        [Column("ip_address")]
        public string? IpAddress { get; set; }

        [Column("current_key_hash")]
        public string CurrentKeyHash { get; set; } = string.Empty;

        [Column("previous_key_hash")]
        public string? PreviousKeyHash { get; set; }

        // Teto absoluto da sessão (login + Institution.RefreshTokenDurationDays) — além
        // dele, rotação não vale mais; exige novo login.
        [Column("expires_at_utc")]
        public DateTime ExpiresAtUtc { get; set; }

        [Column("last_rotated_at_utc")]
        public DateTime LastRotatedAtUtc { get; set; }
    }
}
