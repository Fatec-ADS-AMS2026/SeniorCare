using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Infrastructure;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UnitOfMeasureController : ControllerBase
    {
        private readonly IUnitOfMeasureService _unitOfMeasureService;

        public UnitOfMeasureController(IUnitOfMeasureService service)
        {
            _unitOfMeasureService = service;
        }

        [HttpGet]
        [RequirePermission("UnitOfMeasure", "read")]
        public async Task<ActionResult<PagedResult<UnitOfMeasureDTO>>> Get([FromQuery] CatalogQuery query)
        {
            var unitsOfMeasure = await _unitOfMeasureService.GetAll();
            var filtered = string.IsNullOrWhiteSpace(query.Search)
                ? unitsOfMeasure
                : unitsOfMeasure.Where(u =>
                    u.Description.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                    u.Abbreviation.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
        }

        [HttpGet("{id}")]
        [RequirePermission("UnitOfMeasure", "read")]
        public async Task<ActionResult<UnitOfMeasureDTO>> GetById(int id)
        {
            var unitOfMeasure = await _unitOfMeasureService.GetById(id);
            if (unitOfMeasure == null) throw new KeyNotFoundException("Unidade de medida não encontrada.");
            return Ok(ToDto(unitOfMeasure));
        }

        [HttpPost]
        [RequirePermission("UnitOfMeasure", "write")]
        public async Task<ActionResult<UnitOfMeasureDTO>> Post(UnitOfMeasureCreateRequest request)
        {
            var unitOfMeasure = new UnitOfMeasure { Description = request.Description, Abbreviation = request.Abbreviation };
            await _unitOfMeasureService.Create(unitOfMeasure);
            return CreatedAtAction(nameof(GetById), new { id = unitOfMeasure.Id }, ToDto(unitOfMeasure));
        }

        [HttpPut("{id}")]
        [RequirePermission("UnitOfMeasure", "write")]
        public async Task<ActionResult<UnitOfMeasureDTO>> Put(int id, UnitOfMeasureUpdateRequest request)
        {
            var unitOfMeasure = new UnitOfMeasure { Id = id, Description = request.Description, Abbreviation = request.Abbreviation };
            await _unitOfMeasureService.Update(unitOfMeasure, id, request.RowVersion);
            return Ok(ToDto(unitOfMeasure));
        }

        [HttpDelete("{id}")]
        [RequirePermission("UnitOfMeasure", "delete")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfMeasureService.Remove(id);
            return NoContent();
        }

        private UnitOfMeasureDTO ToDto(UnitOfMeasure unitOfMeasure) => new()
        {
            Id = unitOfMeasure.Id,
            Description = unitOfMeasure.Description,
            Abbreviation = unitOfMeasure.Abbreviation,
            RowVersion = _unitOfMeasureService.GetVersion(unitOfMeasure) ?? 0,
        };
    }
}
