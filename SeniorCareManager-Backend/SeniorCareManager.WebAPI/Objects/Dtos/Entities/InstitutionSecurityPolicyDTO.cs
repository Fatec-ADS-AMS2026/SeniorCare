namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class InstitutionSecurityPolicyDTO
{
    public int? LockoutDurationMinutes { get; set; }
    public int? MaxFailedAttempts { get; set; }
    public int? AccessTokenDurationMinutes { get; set; }
    public int? RefreshTokenDurationDays { get; set; }
    public bool MfaRequiredForAllUsers { get; set; }
}
