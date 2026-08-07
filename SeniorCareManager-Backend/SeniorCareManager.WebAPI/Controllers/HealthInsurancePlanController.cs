using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthInsurancePlanController : ControllerBase
{
    private readonly IHealthInsurancePlanService _healthInsurancePlanService;

    public HealthInsurancePlanController(IHealthInsurancePlanService healthInsurancePlanService)
    {
        _healthInsurancePlanService = healthInsurancePlanService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<HealthInsurancePlanDTO>>> Get()
    {
        var healthInsurancePlans = await _healthInsurancePlanService.GetAll();
        return Ok(healthInsurancePlans.Select(ToDto));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> GetById(int id)
    {
        var healthInsurancePlan = await _healthInsurancePlanService.GetById(id);
        if (healthInsurancePlan == null) throw new KeyNotFoundException("Plano de seguro saúde não encontrado.");
        return Ok(ToDto(healthInsurancePlan));
    }

    [HttpPost]
    public async Task<ActionResult<HealthInsurancePlanDTO>> Post(HealthInsurancePlanCreateRequest request)
    {
        var healthInsurancePlan = new HealthInsurancePlan(0, request.Name, request.Type, request.Abbreviation);
        await _healthInsurancePlanService.Create(healthInsurancePlan);
        return CreatedAtAction(nameof(GetById), new { id = healthInsurancePlan.Id }, ToDto(healthInsurancePlan));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> Put(int id, HealthInsurancePlanUpdateRequest request)
    {
        var healthInsurancePlan = new HealthInsurancePlan(id, request.Name, request.Type, request.Abbreviation);
        await _healthInsurancePlanService.Update(healthInsurancePlan, id);
        return Ok(ToDto(healthInsurancePlan));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _healthInsurancePlanService.Remove(id);
        return NoContent();
    }

    // TODO(3.6): PATCH hoje faz substituição total, idêntico a PUT — remover
    // nesta forma quando a tarefa 3.6 for implementada.
    [HttpPatch("{id}")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> Patch(int id, HealthInsurancePlanUpdateRequest request)
    {
        var healthInsurancePlan = new HealthInsurancePlan(id, request.Name, request.Type, request.Abbreviation);
        await _healthInsurancePlanService.Update(healthInsurancePlan, id);
        return Ok(ToDto(healthInsurancePlan));
    }

    private static HealthInsurancePlanDTO ToDto(HealthInsurancePlan healthInsurancePlan) => new()
    {
        Id = healthInsurancePlan.Id,
        Name = healthInsurancePlan.Name,
        Type = healthInsurancePlan.Type,
        Abbreviation = healthInsurancePlan.Abbreviation,
    };
}
