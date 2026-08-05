using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductGroupController : Controller
{
    private readonly IProductGroupService _productGroupService;

    public ProductGroupController(IProductGroupService service)
    {
        this._productGroupService = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var productGroups = await _productGroupService.GetAll();
        return Ok(productGroups);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var productGroup = await _productGroupService.GetById(id);
        if (productGroup == null) return NotFound("Grupo Produto não encontrado!");
        return Ok(productGroup);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(ProductGroup productGroup)
    {
        try{
            await _productGroupService.Create(productGroup);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo grupo de produto.");
        }
        return Ok(productGroup);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ProductGroup productGroup)
    {
        try
        {
            await _productGroupService.Update(productGroup, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar o grupo de produto: "+ex.Message);
        }
        
        return Ok(productGroup);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productGroupService.Remove(id);
        }
        catch (Exception ex)
        {  
            return StatusCode(500, "Ocorreu um erro ao tentar remover o grupo de produto.");
        }

        return Ok("Grupo de produto apagado com sucesso");
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, ProductGroup productGroup)
    {
        try
        {
            await _productGroupService.Update(productGroup, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover o grupo do produto.");
        }
        
        return Ok(productGroup);
    }

}