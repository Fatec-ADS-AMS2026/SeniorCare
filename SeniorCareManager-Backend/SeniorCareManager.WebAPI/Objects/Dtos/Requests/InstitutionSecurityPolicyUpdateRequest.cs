using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class InstitutionSecurityPolicyUpdateRequest
{
    public int? LockoutDurationMinutes { get; set; }
    public int? MaxFailedAttempts { get; set; }
    public int? AccessTokenDurationMinutes { get; set; }
    public int? RefreshTokenDurationDays { get; set; }
    public bool MfaRequiredForAllUsers { get; set; }

    // Reautenticação (§6.7) — alterar parâmetros de segurança é uma mudança crítica.
    [Required(AllowEmptyStrings = false)]
    public string CurrentPassword { get; set; } = string.Empty;
}
