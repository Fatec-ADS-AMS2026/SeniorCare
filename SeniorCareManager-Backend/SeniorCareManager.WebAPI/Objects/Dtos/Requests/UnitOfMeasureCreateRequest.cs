using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class UnitOfMeasureCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Description { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Abbreviation { get; set; } = string.Empty;
}
