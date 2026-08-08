using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PositionController : ControllerBase
{
    private readonly IPositionService _positionService;

    public PositionController(IPositionService service)
    {
        _positionService = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PositionDTO>>> Get([FromQuery] CatalogQuery query)
    {
        var positions = await _positionService.GetAll();
        var filtered = string.IsNullOrWhiteSpace(query.Search)
            ? positions
            : positions.Where(p => p.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PositionDTO>> GetById(int id)
    {
        var position = await _positionService.GetById(id);
        if (position == null) throw new KeyNotFoundException("Cargo não encontrado.");
        return Ok(ToDto(position));
    }

    [HttpPost]
    public async Task<ActionResult<PositionDTO>> Post(PositionCreateRequest request)
    {
        var position = new Position(0, request.Name);
        await _positionService.Create(position);
        return CreatedAtAction(nameof(GetById), new { id = position.Id }, ToDto(position));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PositionDTO>> Put(int id, PositionUpdateRequest request)
    {
        var position = new Position(id, request.Name);
        await _positionService.Update(position, id, request.RowVersion);
        return Ok(ToDto(position));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _positionService.Remove(id);
        return NoContent();
    }

    private PositionDTO ToDto(Position position) => new()
    {
        Id = position.Id,
        Name = position.Name,
        RowVersion = _positionService.GetVersion(position) ?? 0,
    };
}
