using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ManufacturerCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string CorporateName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string TradeName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string CpfCnpj { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
