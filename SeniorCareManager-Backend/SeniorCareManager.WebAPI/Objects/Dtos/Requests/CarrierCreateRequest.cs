using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class CarrierCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string CorporateName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string TradeName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string CpfCnpj { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressComplement { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
