using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Token { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string NewPassword { get; set; } = string.Empty;
}
