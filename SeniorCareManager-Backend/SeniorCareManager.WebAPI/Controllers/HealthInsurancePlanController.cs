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
public class HealthInsurancePlanController : ControllerBase
{
    private readonly IHealthInsurancePlanService _healthInsurancePlanService;

    public HealthInsurancePlanController(IHealthInsurancePlanService healthInsurancePlanService)
    {
        _healthInsurancePlanService = healthInsurancePlanService;
    }

    [HttpGet]
    [RequirePermission("HealthInsurancePlan", "read")]
    public async Task<ActionResult<PagedResult<HealthInsurancePlanDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var healthInsurancePlans = await _healthInsurancePlanService.GetAll(query.IncludeInactive);
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? healthInsurancePlans
            : healthInsurancePlans.Where(h =>
                h.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                h.Abbreviation.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    [RequirePermission("HealthInsurancePlan", "read")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> GetById(int id)
    {
        var healthInsurancePlan = await _healthInsurancePlanService.GetById(id);
        if (healthInsurancePlan == null) throw new KeyNotFoundException("Plano de seguro saúde não encontrado.");
        return Ok(ToDto(healthInsurancePlan));
    }

    [HttpPost]
    [RequirePermission("HealthInsurancePlan", "write")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> Post(HealthInsurancePlanCreateRequest request)
    {
        var healthInsurancePlan = new HealthInsurancePlan(0, request.Name, request.Type, request.Abbreviation);
        await _healthInsurancePlanService.Create(healthInsurancePlan);
        return CreatedAtAction(nameof(GetById), new { id = healthInsurancePlan.Id }, ToDto(healthInsurancePlan));
    }

    [HttpPut("{id}")]
    [RequirePermission("HealthInsurancePlan", "write")]
    public async Task<ActionResult<HealthInsurancePlanDTO>> Put(int id, HealthInsurancePlanUpdateRequest request)
    {
        var healthInsurancePlan = new HealthInsurancePlan(id, request.Name, request.Type, request.Abbreviation);
        await _healthInsurancePlanService.Update(healthInsurancePlan, id, request.RowVersion);
        return Ok(ToDto(healthInsurancePlan));
    }

    [HttpDelete("{id}")]
    [RequirePermission("HealthInsurancePlan", "delete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _healthInsurancePlanService.Remove(id);
        return NoContent();
    }

    [HttpPut("{id}/activate")]
    [RequirePermission("HealthInsurancePlan", "write")]
    public async Task<IActionResult> Activate(int id)
    {
        await _healthInsurancePlanService.Activate(id);
        return NoContent();
    }

    private HealthInsurancePlanDTO ToDto(HealthInsurancePlan healthInsurancePlan) => new()
    {
        Id = healthInsurancePlan.Id,
        Name = healthInsurancePlan.Name,
        Type = healthInsurancePlan.Type,
        Abbreviation = healthInsurancePlan.Abbreviation,
        RowVersion = _healthInsurancePlanService.GetVersion(healthInsurancePlan) ?? 0,
        IsActive = healthInsurancePlan.IsActive,
    };
}
