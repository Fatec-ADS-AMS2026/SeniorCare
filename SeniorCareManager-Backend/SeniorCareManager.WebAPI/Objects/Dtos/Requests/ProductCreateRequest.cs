using System.ComponentModel.DataAnnotations;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Requests;

public class ProductCreateRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Description { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string GenericName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "ProductTypeId deve referenciar um tipo de produto ativo.")]
    public int ProductTypeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "UnitOfMeasureId deve referenciar uma unidade de medida ativa.")]
    public int UnitOfMeasureId { get; set; }

    // Não usar Range(0, double.MaxValue) aqui: RangeAttribute converte os limites pro tipo
    // da propriedade, e double.MaxValue estoura ao converter pra decimal (OverflowException,
    // rejeita qualquer valor). int.MaxValue cabe em decimal sem overflow e já é limite mais
    // que suficiente pra estoque/preço.
    [Range(0, int.MaxValue)]
    public decimal MinimumStock { get; set; }

    [Range(0, int.MaxValue)]
    public decimal CurrentStock { get; set; }

    [Range(0, int.MaxValue)]
    public decimal UnitPrice { get; set; }

    public decimal? AverageCost { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public decimal? StockValue { get; set; }
    public bool HighCost { get; set; }
    public bool ExpirationControlled { get; set; }
}
