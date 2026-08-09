using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class RegenerateRecoveryCodesRequest
{
    // Reautenticação (§6.7/§7.5) — regenerar códigos invalida os anteriores.
    [Required(AllowEmptyStrings = false)]
    public string CurrentPassword { get; set; } = string.Empty;
}
