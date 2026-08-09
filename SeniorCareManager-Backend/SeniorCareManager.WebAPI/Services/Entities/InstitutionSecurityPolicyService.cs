using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Services.Entities;

public class InstitutionSecurityPolicyService : IInstitutionSecurityPolicyService
{
    public const int MinLockoutDurationMinutes = 1;
    public const int MaxLockoutDurationMinutes = 1440;

    public const int MinFailedAttempts = 3;
    public const int MaxFailedAttempts = 20;

    public const int MinAccessTokenDurationMinutes = 1;
    public const int MaxAccessTokenDurationMinutes = 60;

    public const int MinRefreshTokenDurationDays = 1;
    public const int MaxRefreshTokenDurationDays = 90;

    // Valores seguros quando a instituição não configurou nada (§7.8) — dentro das faixas
    // acima por construção.
    public const int DefaultMaxFailedAttempts = 5;
    public const int DefaultLockoutDurationMinutes = 15;

    public void ValidateSecuritySettings(
        int? lockoutDurationMinutes,
        int? maxFailedAttempts,
        int? accessTokenDurationMinutes,
        int? refreshTokenDurationDays)
    {
        ValidateRange(lockoutDurationMinutes, MinLockoutDurationMinutes, MaxLockoutDurationMinutes, "duração de bloqueio (minutos)");
        ValidateRange(maxFailedAttempts, MinFailedAttempts, MaxFailedAttempts, "limite de tentativas com falha");
        ValidateRange(accessTokenDurationMinutes, MinAccessTokenDurationMinutes, MaxAccessTokenDurationMinutes, "duração do acesso (minutos)");
        ValidateRange(refreshTokenDurationDays, MinRefreshTokenDurationDays, MaxRefreshTokenDurationDays, "duração da renovação (dias)");
    }

    private static void ValidateRange(int? value, int min, int max, string label)
    {
        if (value.HasValue && (value.Value < min || value.Value > max))
            throw new BusinessRuleException($"O parâmetro '{label}' deve estar entre {min} e {max}.");
    }
}
