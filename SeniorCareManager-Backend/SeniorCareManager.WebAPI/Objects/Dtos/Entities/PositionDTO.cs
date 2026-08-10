namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities
{
    public class PositionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public uint RowVersion { get; set; }
        public bool IsActive { get; set; }
    }
}
