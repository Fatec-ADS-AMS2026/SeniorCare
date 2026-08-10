namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class ProductTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ProductGroupId { get; set; }
    public uint RowVersion { get; set; }
        public bool IsActive { get; set; }
}
