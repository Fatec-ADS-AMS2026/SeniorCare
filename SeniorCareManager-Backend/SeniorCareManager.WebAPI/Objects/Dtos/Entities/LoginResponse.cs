namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class LoginResponse
{
    // "ok" | "mfa_required" | "mfa_enrollment_required"
    public string Status { get; set; } = string.Empty;

    public string? ChallengeToken { get; set; }

    public CurrentIdentityDTO? Identity { get; set; }

    // Só preenchido quando o login foi concluído com um código de recuperação — alerta a
    // quantidade restante (spec: "Código de recuperação").
    public int? RemainingRecoveryCodes { get; set; }
}
