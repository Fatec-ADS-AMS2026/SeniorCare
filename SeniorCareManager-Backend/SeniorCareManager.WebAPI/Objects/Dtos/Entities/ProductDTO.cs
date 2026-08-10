namespace SeniorCareManager.WebAPI.Objects.Dtos.Entities;

public class ProductDTO
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public int ProductTypeId { get; set; }
    public int UnitOfMeasureId { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? AverageCost { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public decimal? StockValue { get; set; }
    public bool HighCost { get; set; }
    public bool ExpirationControlled { get; set; }
    public uint RowVersion { get; set; }
    public bool IsActive { get; set; }
}
