namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class OrganizationalRoleUpdateRequest : OrganizationalRoleCreateRequest
{
    public uint RowVersion { get; set; }
}
