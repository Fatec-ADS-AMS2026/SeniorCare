using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;
[ApiController]
[Route("api/v1/[controller]")]
public class ReligionController : Controller
{
    private readonly IReligionService _religionService;

    public ReligionController(IReligionService service)
    {
        this._religionService = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var religion = await _religionService.GetAll();

        if (religion == null)
        {
            return StatusCode(500, $"Nenhuma religião encontrada!");
        }

        else
            return Ok(religion);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var religionId = await _religionService.GetById(id);
        if (religionId == null) return NotFound("Religião não encontrda!");
        return Ok(religionId);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Religion religion)
    {
        if (religion.Name == string.Empty)
            return StatusCode(500, $"O nome da religião não pode ser nulo!");

        if (religion.Id < 0)
            return StatusCode(500, "O id da religião não pode ser inferior a 0!");

        try
        {
            await _religionService.Create(religion);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ocorreu um erro ao tentar inserir uma nova Religião! {ex}");
        }
        return Ok(religion);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, Religion religion)
    {
        try
        {
            await _religionService.Update(religion, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar a religião: " + ex.Message);
        }

        return Ok(religion);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _religionService.Remove(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover a religião.");
        }

        return Ok("Grupo de religião apagado com sucesso");
    }
}