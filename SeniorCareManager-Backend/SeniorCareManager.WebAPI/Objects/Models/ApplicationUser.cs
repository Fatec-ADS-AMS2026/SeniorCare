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

        // Reservado a operações do sistema (spec §5) — deliberadamente fora do sistema de
        // Role/PermissionGroup para nunca ser atribuível por nenhuma API administrativa,
        // só por provisionamento direto.
        public bool IsSystemAdmin { get; set; }
    }
}
