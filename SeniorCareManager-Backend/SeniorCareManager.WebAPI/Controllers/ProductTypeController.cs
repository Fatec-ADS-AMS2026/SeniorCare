using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductTypeController : Controller
{
    private readonly IProductTypeService _productTypeService;

    public ProductTypeController(IProductTypeService service)
    {
        this._productTypeService = service;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var productTypes = await _productTypeService.GetAll();
        return Ok(productTypes);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var productType = await _productTypeService.GetById(id);
        if (productType == null) return NotFound("Tipo Produto não encontrado!");
        return Ok(productType);
    }
    
    [HttpPost]
    public async Task<IActionResult> Post(ProductType productType)
    {
        try{
            await _productTypeService.Create(productType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo tipo de produto.");
        }
        return Ok(productType);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ProductType productType)
    {
        try
        {
            await _productTypeService.Update(productType, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar o tipo de produto: "+ex.Message);
        }
        
        return Ok(productType);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productTypeService.Remove(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover o tipo de produto.");
        }

        return Ok("Tipo de produto apagado com sucesso");
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, ProductType productType)
    {
        try
        {
            await _productTypeService.Update(productType, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover o tipo do produto.");
        }
        
        return Ok(productType);
    }

}