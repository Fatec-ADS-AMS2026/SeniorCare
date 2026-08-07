using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<IEnumerable<UnitOfMeasureDTO>>> Get()
        {
            var unitsOfMeasure = await _unitOfMeasureService.GetAll();
            return Ok(unitsOfMeasure.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UnitOfMeasureDTO>> GetById(int id)
        {
            var unitOfMeasure = await _unitOfMeasureService.GetById(id);
            if (unitOfMeasure == null) throw new KeyNotFoundException("Unidade de medida não encontrada.");
            return Ok(ToDto(unitOfMeasure));
        }

        [HttpPost]
        public async Task<ActionResult<UnitOfMeasureDTO>> Post(UnitOfMeasureCreateRequest request)
        {
            var unitOfMeasure = new UnitOfMeasure { Description = request.Description, Abbreviation = request.Abbreviation };
            await _unitOfMeasureService.Create(unitOfMeasure);
            return CreatedAtAction(nameof(GetById), new { id = unitOfMeasure.Id }, ToDto(unitOfMeasure));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UnitOfMeasureDTO>> Put(int id, UnitOfMeasureUpdateRequest request)
        {
            var unitOfMeasure = new UnitOfMeasure { Id = id, Description = request.Description, Abbreviation = request.Abbreviation };
            await _unitOfMeasureService.Update(unitOfMeasure, id);
            return Ok(ToDto(unitOfMeasure));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitOfMeasureService.Remove(id);
            return NoContent();
        }

        private static UnitOfMeasureDTO ToDto(UnitOfMeasure unitOfMeasure) => new()
        {
            Id = unitOfMeasure.Id,
            Description = unitOfMeasure.Description,
            Abbreviation = unitOfMeasure.Abbreviation,
        };
    }
}
