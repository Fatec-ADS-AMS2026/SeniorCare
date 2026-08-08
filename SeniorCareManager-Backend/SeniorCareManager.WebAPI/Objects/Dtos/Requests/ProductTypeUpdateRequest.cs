namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ProductTypeUpdateRequest : ProductTypeCreateRequest
{
    public uint RowVersion { get; set; }
}
