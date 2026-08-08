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

        public Institution() { }

        public Institution(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
