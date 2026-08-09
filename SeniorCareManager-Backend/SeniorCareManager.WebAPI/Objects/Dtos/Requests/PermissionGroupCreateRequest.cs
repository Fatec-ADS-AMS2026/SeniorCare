using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class PermissionGroupCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;
}
