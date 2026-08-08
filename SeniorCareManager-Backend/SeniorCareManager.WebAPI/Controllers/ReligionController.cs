using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ReligionController : ControllerBase
{
    private readonly IReligionService _religionService;

    public ReligionController(IReligionService service)
    {
        _religionService = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ReligionDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var religions = await _religionService.GetAll();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? religions
            : religions.Where(r => r.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReligionDTO>> GetById(int id)
    {
        var religion = await _religionService.GetById(id);
        if (religion == null) throw new KeyNotFoundException("Religião não encontrada.");
        return Ok(ToDto(religion));
    }

    [HttpPost]
    public async Task<ActionResult<ReligionDTO>> Post(ReligionCreateRequest request)
    {
        var religion = new Religion { Name = request.Name };
        await _religionService.Create(religion);
        return CreatedAtAction(nameof(GetById), new { id = religion.Id }, ToDto(religion));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ReligionDTO>> Put(int id, ReligionUpdateRequest request)
    {
        var religion = new Religion { Id = id, Name = request.Name };
        await _religionService.Update(religion, id, request.RowVersion);
        return Ok(ToDto(religion));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _religionService.Remove(id);
        return NoContent();
    }

    private ReligionDTO ToDto(Religion religion) => new()
    {
        Id = religion.Id,
        Name = religion.Name,
        RowVersion = _religionService.GetVersion(religion) ?? 0,
    };
}
