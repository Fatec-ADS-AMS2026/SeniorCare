using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos;
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
    public async Task<ActionResult<IEnumerable<ProductGroupDTO>>> Get()
    {
        var productGroups = await _productGroupService.GetAll();
        return Ok(productGroups.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductGroupDTO>> GetById(int id)
    {
        var productGroup = await _productGroupService.GetById(id);
        if (productGroup == null) throw new KeyNotFoundException("Grupo de produto não encontrado.");
        return Ok(ToDto(productGroup));
    }

    [HttpPost]
    public async Task<ActionResult<ProductGroupDTO>> Post(ProductGroupCreateRequest request)
    {
        var productGroup = new ProductGroup { Name = request.Name };
        await _productGroupService.Create(productGroup);
        return CreatedAtAction(nameof(GetById), new { id = productGroup.Id }, ToDto(productGroup));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductGroupDTO>> Put(int id, ProductGroupUpdateRequest request)
    {
        var productGroup = new ProductGroup { Id = id, Name = request.Name };
        await _productGroupService.Update(productGroup, id);
        return Ok(ToDto(productGroup));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productGroupService.Remove(id);
        return NoContent();
    }

    // TODO(3.6): PATCH hoje faz substituição total, idêntico a PUT — remover
    // nesta forma quando a tarefa 3.6 (ID de rota canônico / remoção do PATCH
    // de substituição total) for implementada.
    [HttpPatch("{id}")]
    public async Task<ActionResult<ProductGroupDTO>> Patch(int id, ProductGroupUpdateRequest request)
    {
        var productGroup = new ProductGroup { Id = id, Name = request.Name };
        await _productGroupService.Update(productGroup, id);
        return Ok(ToDto(productGroup));
    }

    private static ProductGroupDTO ToDto(ProductGroup productGroup) => new()
    {
        Id = productGroup.Id,
        Name = productGroup.Name,
    };
}
