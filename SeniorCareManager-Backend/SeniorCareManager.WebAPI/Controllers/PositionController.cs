using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PositionController: Controller
{
    private readonly IPositionService _positionService;

    public PositionController(IPositionService service)
    {
        this._positionService = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var position = await _positionService.GetAll();
        return Ok(position);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var position = await _positionService.GetById(id);
        if (position == null) return NotFound("Cargo não encontrado!");
        return Ok(position);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Position position)
    {
        try
        {
            await _positionService.Create(position);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo cargo.");
        }
        return Ok(position);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Position position)
    {
        try
        {
            await _positionService.Update(position, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar o cargo: " + ex.Message);
        }

        return Ok(position);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _positionService.Remove(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover o cargo.");
        }

        return Ok("cargo apagado com sucesso");
    }


}