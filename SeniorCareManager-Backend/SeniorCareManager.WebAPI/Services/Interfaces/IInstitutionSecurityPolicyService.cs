namespace SeniorCareManager.WebAPI.Services.Interfaces;

public interface IInstitutionSecurityPolicyService
{
    // Valida faixas mínimas/máximas seguras; lança BusinessRuleException fora delas. Campos
    // nulos não são validados (usam o default seguro do provedor quando a §7 consumir).
    void ValidateSecuritySettings(
        int? lockoutDurationMinutes,
        int? maxFailedAttempts,
        int? accessTokenDurationMinutes,
        int? refreshTokenDurationDays);
}
