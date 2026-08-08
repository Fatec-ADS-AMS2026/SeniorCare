using System;
using Microsoft.AspNetCore.Identity;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid InstitutionId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public IdentityOrigin IdentityOrigin { get; set; } = IdentityOrigin.LOCAL;

        public AccountState AccountState { get; set; } = AccountState.PROVISIONED;

        // TOTP real chega na §7; o flag já existe agora porque o piso de senha (4.5) depende
        // de saber se a conta tem MFA obrigatório.
        public bool MfaEnabled { get; set; }
    }
}
