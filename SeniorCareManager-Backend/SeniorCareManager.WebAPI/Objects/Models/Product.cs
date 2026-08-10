using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SeniorCareManager.WebAPI.Objects.Models;

// §9.4-9.5: catálogo, não saldo transacional — campos de estoque/custo são gravados
// diretamente (design.md decisão 5), sem tabela de movimento/lote.
[Table("product")]
public class Product
{
    [Column("id")]
    public int Id { get; set; }

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("generic_name")]
    public string GenericName { get; set; } = string.Empty;

    [Column("product_type_id")][ForeignKey("producttype")]
    public int ProductTypeId { get; set; }

    [Column("unit_of_measure_id")][ForeignKey("unitofmeasure")]
    public int UnitOfMeasureId { get; set; }

    [Column("minimum_stock")]
    public decimal MinimumStock { get; set; }

    [Column("current_stock")]
    public decimal CurrentStock { get; set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("average_cost")]
    public decimal? AverageCost { get; set; }

    [Column("last_purchase_price")]
    public decimal? LastPurchasePrice { get; set; }

    [Column("stock_value")]
    public decimal? StockValue { get; set; }

    [Column("high_cost")]
    public bool HighCost { get; set; }

    [Column("expiration_controlled")]
    public bool ExpirationControlled { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public ProductType? ProductType { get; set; }

    [JsonIgnore]
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public Product() { }

    public Product(
        int id, string description, string genericName, int productTypeId, int unitOfMeasureId,
        decimal minimumStock, decimal currentStock, decimal unitPrice, decimal? averageCost,
        decimal? lastPurchasePrice, decimal? stockValue, bool highCost, bool expirationControlled)
    {
        Id = id;
        Description = description;
        GenericName = genericName;
        ProductTypeId = productTypeId;
        UnitOfMeasureId = unitOfMeasureId;
        MinimumStock = minimumStock;
        CurrentStock = currentStock;
        UnitPrice = unitPrice;
        AverageCost = averageCost;
        LastPurchasePrice = lastPurchasePrice;
        StockValue = stockValue;
        HighCost = highCost;
        ExpirationControlled = expirationControlled;
    }
}
