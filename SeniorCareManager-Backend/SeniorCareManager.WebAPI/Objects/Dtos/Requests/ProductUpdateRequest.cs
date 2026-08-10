namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ProductUpdateRequest : ProductCreateRequest
{
    public uint RowVersion { get; set; }
}
