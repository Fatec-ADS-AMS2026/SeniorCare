using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class LoginMfaRequest
{
    [Required(AllowEmptyStrings = false)]
    public string ChallengeToken { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Code { get; set; } = string.Empty;
}
