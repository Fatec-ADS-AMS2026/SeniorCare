using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class RecoverAccountRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
