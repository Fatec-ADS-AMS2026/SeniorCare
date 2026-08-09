using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    // Mínima de propósito — ninguém cria linha aqui até a §7 implementar login de verdade.
    // A §7 deve estender esta tabela (colunas aditivas: hash do token de renovação, cadeia
    // de rotação) em vez de recriá-la.
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
    }
}
