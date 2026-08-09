using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    [Table("institution")]
    public class Institution
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        // Nulo = usa o piso padrão (15 sem MFA / 8 com MFA). Só pode ser configurado para um
        // valor maior que o piso — validado em PasswordPolicyService, nunca aqui.
        [Column("min_password_length_without_mfa_override")]
        public int? MinPasswordLengthWithoutMfaOverride { get; set; }

        [Column("min_password_length_with_mfa_override")]
        public int? MinPasswordLengthWithMfaOverride { get; set; }

        // Parâmetros de segurança institucionais (§6.5) — nulo usa o default seguro;
        // InstitutionSecurityPolicyService valida faixas mínimas/máximas. Consumidos de fato
        // pela §7 (login/MFA/rate limit ainda não existem).
        [Column("lockout_duration_minutes")]
        public int? LockoutDurationMinutes { get; set; }

        [Column("max_failed_attempts")]
        public int? MaxFailedAttempts { get; set; }

        [Column("access_token_duration_minutes")]
        public int? AccessTokenDurationMinutes { get; set; }

        [Column("refresh_token_duration_days")]
        public int? RefreshTokenDurationDays { get; set; }

        // Só pode ir de false para true — fortalece, nunca enfraquece o piso fixo "MFA
        // obrigatório para admin", que não é configurável.
        [Column("mfa_required_for_all_users")]
        public bool MfaRequiredForAllUsers { get; set; }

        public Institution() { }

        public Institution(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
