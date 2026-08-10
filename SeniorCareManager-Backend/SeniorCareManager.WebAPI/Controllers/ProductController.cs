using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

// Produto como catálogo (§9.4-9.6, design.md decisão 5) — mesmo padrão dos outros 9
// catálogos (§3b), sem tabela de movimento/lote/saldo derivado.
[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProductTypeService _productTypeService;
    private readonly IUnitOfMeasureService _unitOfMeasureService;

    public ProductController(
        IProductService productService, IProductTypeService productTypeService, IUnitOfMeasureService unitOfMeasureService)
    {
        _productService = productService;
        _productTypeService = productTypeService;
        _unitOfMeasureService = unitOfMeasureService;
    }

    [HttpGet]
    [RequirePermission("Product", "read")]
    public async Task<ActionResult<PagedResult<ProductDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var products = await _productService.GetAll(query.IncludeInactive);
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? products
            : products.Where(p =>
                p.Description.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                p.GenericName.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("Product", "read")]
    public async Task<ActionResult<ProductDTO>> GetById(int id)
    {
        var product = await _productService.GetById(id);
        if (product == null) throw new KeyNotFoundException("Produto não encontrado.");
        return Ok(ToDto(product));
    }

    [HttpPost]
    [RequirePermission("Product", "write")]
    public async Task<ActionResult<ProductDTO>> Post(ProductCreateRequest request)
    {
        await EnsureReferencesActiveAsync(request.ProductTypeId, request.UnitOfMeasureId);
        var product = ToModel(0, request);
        await _productService.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToDto(product));
    }

    [HttpPut("{id}")]
    [RequirePermission("Product", "write")]
    public async Task<ActionResult<ProductDTO>> Put(int id, ProductUpdateRequest request)
    {
        await EnsureReferencesActiveAsync(request.ProductTypeId, request.UnitOfMeasureId);
        var product = ToModel(id, request);
        await _productService.Update(product, id, request.RowVersion);
        return Ok(ToDto(product));
    }

    [HttpDelete("{id}")]
    [RequirePermission("Product", "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.Remove(id);
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [RequirePermission("Product", "write")]
    public async Task<IActionResult> Activate(int id)
    {
        await _productService.Activate(id);
        return NoContent();
    }

    // §9.3: não basta existir — tipo e unidade de medida referenciados precisam estar ativos.
    private async Task EnsureReferencesActiveAsync(int productTypeId, int unitOfMeasureId)
    {
        var productType = await _productTypeService.GetById(productTypeId);
        if (productType == null || !productType.IsActive)
            throw new BusinessRuleException($"ProductTypeId {productTypeId} não referencia um tipo de produto ativo.");

        var unitOfMeasure = await _unitOfMeasureService.GetById(unitOfMeasureId);
        if (unitOfMeasure == null || !unitOfMeasure.IsActive)
            throw new BusinessRuleException($"UnitOfMeasureId {unitOfMeasureId} não referencia uma unidade de medida ativa.");
    }

    private static Product ToModel(int id, ProductCreateRequest request) => new(
        id, request.Description, request.GenericName, request.ProductTypeId, request.UnitOfMeasureId,
        request.MinimumStock, request.CurrentStock, request.UnitPrice, request.AverageCost,
        request.LastPurchasePrice, request.StockValue, request.HighCost, request.ExpirationControlled);

    private ProductDTO ToDto(Product product) => new()
    {
        Id = product.Id,
        Description = product.Description,
        GenericName = product.GenericName,
        ProductTypeId = product.ProductTypeId,
        UnitOfMeasureId = product.UnitOfMeasureId,
        MinimumStock = product.MinimumStock,
        CurrentStock = product.CurrentStock,
        UnitPrice = product.UnitPrice,
        AverageCost = product.AverageCost,
        LastPurchasePrice = product.LastPurchasePrice,
        StockValue = product.StockValue,
        HighCost = product.HighCost,
        ExpirationControlled = product.ExpirationControlled,
        RowVersion = _productService.GetVersion(product) ?? 0,
        IsActive = product.IsActive,
    };
}
