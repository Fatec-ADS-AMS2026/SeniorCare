using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductGroupController : ControllerBase
{
    private readonly IProductGroupService _productGroupService;

    public ProductGroupController(IProductGroupService service)
    {
        _productGroupService = service;
    }

    [HttpGet]
    [RequirePermission("ProductGroup", "read")]
    public async Task<ActionResult<PagedResult<ProductGroupDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var productGroups = await _productGroupService.GetAll(query.IncludeInactive);
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? productGroups
            : productGroups.Where(g => g.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("ProductGroup", "read")]
    public async Task<ActionResult<ProductGroupDTO>> GetById(int id)
    {
        var productGroup = await _productGroupService.GetById(id);
        if (productGroup == null) throw new KeyNotFoundException("Grupo de produto não encontrado.");
        return Ok(ToDto(productGroup));
    }

    [HttpPost]
    [RequirePermission("ProductGroup", "write")]
    public async Task<ActionResult<ProductGroupDTO>> Post(ProductGroupCreateRequest request)
    {
        var productGroup = new ProductGroup { Name = request.Name };
        await _productGroupService.Create(productGroup);
        return CreatedAtAction(nameof(GetById), new { id = productGroup.Id }, ToDto(productGroup));
    }

    [HttpPut("{id}")]
    [RequirePermission("ProductGroup", "write")]
    public async Task<ActionResult<ProductGroupDTO>> Put(int id, ProductGroupUpdateRequest request)
    {
        var productGroup = new ProductGroup { Id = id, Name = request.Name };
        await _productGroupService.Update(productGroup, id, request.RowVersion);
        return Ok(ToDto(productGroup));
    }

    [HttpDelete("{id}")]
    [RequirePermission("ProductGroup", "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productGroupService.Remove(id);
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [RequirePermission("ProductGroup", "write")]
    public async Task<IActionResult> Activate(int id)
    {
        await _productGroupService.Activate(id);
        return NoContent();
    }

    private ProductGroupDTO ToDto(ProductGroup productGroup) => new()
    {
        Id = productGroup.Id,
        Name = productGroup.Name,
        RowVersion = _productGroupService.GetVersion(productGroup) ?? 0,
        IsActive = productGroup.IsActive,
    };
}
