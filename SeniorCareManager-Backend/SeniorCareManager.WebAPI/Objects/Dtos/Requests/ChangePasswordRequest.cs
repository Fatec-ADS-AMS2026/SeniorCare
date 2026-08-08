using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ChangePasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string NewPassword { get; set; } = string.Empty;
}
