using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthInsurancePlanController : Controller
{
    private readonly IHealthInsurancePlanService _healthInsurancePlanService;

    public HealthInsurancePlanController(IHealthInsurancePlanService healthInsurancePlanService)
    {
        this._healthInsurancePlanService = healthInsurancePlanService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var healthInsurancePlans = await _healthInsurancePlanService.GetAll();
        return Ok(healthInsurancePlans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var healthInsurancePlan = await _healthInsurancePlanService.GetById(id);
        if (healthInsurancePlan == null) return NotFound("Plano de seguro saúde não encontrado!");
        return Ok(healthInsurancePlan);
    }

    [HttpPost]
    public async Task<IActionResult> Post(HealthInsurancePlan healthInsurancePlan)
    {
        try
        {
            await _healthInsurancePlanService.Create(healthInsurancePlan);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo plano de seguro saúde.");
        }
        return Ok(healthInsurancePlan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, HealthInsurancePlan healthInsurancePlan)
    {
        try
        {
            await _healthInsurancePlanService.Update(healthInsurancePlan, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar o plano de seguro saúde: " + ex.Message);
        }

        return Ok(healthInsurancePlan);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _healthInsurancePlanService.Remove(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar remover o plano de seguro saúde.");
        }

        return Ok("Plano de seguro saúde apagado com sucesso");
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, HealthInsurancePlan healthInsurancePlan)
    {
        try
        {
            await _healthInsurancePlanService.Update(healthInsurancePlan, id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Ocorreu um erro ao tentar atualizar o plano de seguro saúde.");
        }

        return Ok(healthInsurancePlan);
    }
}
