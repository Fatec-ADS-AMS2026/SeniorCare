namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class SupplierUpdateRequest : SupplierCreateRequest
{
    public uint RowVersion { get; set; }
}
