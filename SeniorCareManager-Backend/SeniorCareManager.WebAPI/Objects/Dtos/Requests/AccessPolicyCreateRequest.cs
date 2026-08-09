using System.ComponentModel.DataAnnotations;
using SeniorCareManager.WebAPI.Objects.Enums;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class AccessPolicyCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Resource { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Action { get; set; } = string.Empty;

    public string? Feature { get; set; }

    public AccessScopeType? ScopeType { get; set; }

    public string? ScopeKey { get; set; }

    [Required]
    public AccessEffect Effect { get; set; }
}
