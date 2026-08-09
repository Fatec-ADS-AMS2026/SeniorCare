namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class MfaEnrollResponse
{
    public string AuthenticatorKey { get; set; } = string.Empty;

    public string OtpAuthUri { get; set; } = string.Empty;
}
