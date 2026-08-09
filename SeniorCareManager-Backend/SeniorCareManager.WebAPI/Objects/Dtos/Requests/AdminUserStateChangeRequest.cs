using System.ComponentModel.DataAnnotations;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class AdminUserStateChangeRequest
{
    [Required]
    public AccountState AccountState { get; set; }

    // Reautenticação da pessoa que executa a ação (§6.7) — nunca a senha do alvo.
    [Required(AllowEmptyStrings = false)]
    public string CurrentPassword { get; set; } = string.Empty;
}
