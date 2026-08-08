namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ProductGroupUpdateRequest : ProductGroupCreateRequest
{
    public uint RowVersion { get; set; }
}
