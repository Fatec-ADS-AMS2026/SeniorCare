using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class MfaConfirmRequest
{
    public string? ChallengeToken { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Code { get; set; } = string.Empty;
}
