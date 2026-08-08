using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductTypeController : ControllerBase
{
    private readonly IProductTypeService _productTypeService;
    private readonly IProductGroupService _productGroupService;

    public ProductTypeController(IProductTypeService service, IProductGroupService productGroupService)
    {
        _productTypeService = service;
        _productGroupService = productGroupService;
    }

    [HttpGet]
    [RequirePermission("ProductType", "read")]
    public async Task<ActionResult<PagedResult<ProductTypeDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var productTypes = await _productTypeService.GetAll();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? productTypes
            : productTypes.Where(t => t.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("ProductType", "read")]
    public async Task<ActionResult<ProductTypeDTO>> GetById(int id)
    {
        var productType = await _productTypeService.GetById(id);
        if (productType == null) throw new KeyNotFoundException("Tipo de produto não encontrado.");
        return Ok(ToDto(productType));
    }

    [HttpPost]
    [RequirePermission("ProductType", "write")]
    public async Task<ActionResult<ProductTypeDTO>> Post(ProductTypeCreateRequest request)
    {
        await EnsureProductGroupExists(request.ProductGroupId);
        var productType = new ProductType(0, request.Name, request.ProductGroupId);
        await _productTypeService.Create(productType);
        return CreatedAtAction(nameof(GetById), new { id = productType.Id }, ToDto(productType));
    }

    [HttpPut("{id}")]
    [RequirePermission("ProductType", "write")]
    public async Task<ActionResult<ProductTypeDTO>> Put(int id, ProductTypeUpdateRequest request)
    {
        await EnsureProductGroupExists(request.ProductGroupId);
        var productType = new ProductType(id, request.Name, request.ProductGroupId);
        await _productTypeService.Update(productType, id, request.RowVersion);
        return Ok(ToDto(productType));
    }

    [HttpDelete("{id}")]
    [RequirePermission("ProductType", "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productTypeService.Remove(id);
        return NoContent();
    }

    private async Task EnsureProductGroupExists(int productGroupId)
    {
        var productGroup = await _productGroupService.GetById(productGroupId);
        if (productGroup == null)
        {
            throw new BusinessRuleException($"ProductGroupId {productGroupId} não referencia um grupo de produto existente.");
        }
    }

    private ProductTypeDTO ToDto(ProductType productType) => new()
    {
        Id = productType.Id,
        Name = productType.Name,
        ProductGroupId = productType.ProductGroupId,
        RowVersion = _productTypeService.GetVersion(productType) ?? 0,
    };
}
