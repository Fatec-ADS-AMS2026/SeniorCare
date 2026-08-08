using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IInstitutionIdentityOriginService
{
    // Lança BusinessRuleException para LDAP/OIDC — pontos de extensão sem provedor
    // configurado ainda. Não simula integração inexistente (design.md decisão 6).
    void EnsureOriginAvailable(IdentityOrigin origin);
}
