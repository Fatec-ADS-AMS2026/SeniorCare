namespace SeniorCareManager.WebAPI.Objects.Dtos;

public class ProductGroupDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public uint RowVersion { get; set; }
    public bool IsActive { get; set; }
}