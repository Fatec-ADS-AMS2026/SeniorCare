using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class RevokeSessionRequest
{
    // Reautenticação (§6.7) — revogar sessão é uma mudança crítica.
    [Required(AllowEmptyStrings = false)]
    public string CurrentPassword { get; set; } = string.Empty;
}
