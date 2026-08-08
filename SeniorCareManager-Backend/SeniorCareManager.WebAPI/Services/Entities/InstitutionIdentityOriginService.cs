using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Enums;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class InstitutionIdentityOriginService : IInstitutionIdentityOriginService
{
    public void EnsureOriginAvailable(IdentityOrigin origin)
    {
        if (origin == IdentityOrigin.LOCAL)
            return;

        throw new BusinessRuleException(
            $"A origem de identidade {origin} não está disponível: nenhum provedor {origin} está configurado nesta instalação.");
    }
}
