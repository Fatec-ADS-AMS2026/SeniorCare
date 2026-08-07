using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ManufacturerController : ControllerBase
    {
        private readonly IManufacturerService _manufacturerService;

        public ManufacturerController(IManufacturerService service)
        {
            _manufacturerService = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ManufacturerDTO>>> Get([FromQuery] CatalogQuery query)
        {
            var manufacturers = await _manufacturerService.GetAll();
            var filtered = string.IsNullOrWhiteSpace(query.Search)
                ? manufacturers
                : manufacturers.Where(m =>
                    m.CorporateName.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                    m.TradeName.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ManufacturerDTO>> GetById(int id)
        {
            var manufacturer = await _manufacturerService.GetById(id);
            if (manufacturer == null) throw new KeyNotFoundException("Fabricante não encontrado.");
            return Ok(ToDto(manufacturer));
        }

        [HttpPost]
        public async Task<ActionResult<ManufacturerDTO>> Post(ManufacturerCreateRequest request)
        {
            var manufacturer = new Manufacturer(0, request.CorporateName, request.TradeName, request.CpfCnpj, request.Phone, request.Email);
            await _manufacturerService.Create(manufacturer);
            return CreatedAtAction(nameof(GetById), new { id = manufacturer.Id }, ToDto(manufacturer));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ManufacturerDTO>> Put(int id, ManufacturerUpdateRequest request)
        {
            var manufacturer = new Manufacturer(id, request.CorporateName, request.TradeName, request.CpfCnpj, request.Phone, request.Email);
            await _manufacturerService.Update(manufacturer, id);
            return Ok(ToDto(manufacturer));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _manufacturerService.Remove(id);
            return NoContent();
        }

        private static ManufacturerDTO ToDto(Manufacturer manufacturer) => new()
        {
            Id = manufacturer.Id,
            CorporateName = manufacturer.CorporateName,
            TradeName = manufacturer.TradeName,
            CpfCnpj = manufacturer.CpfCnpj,
            Phone = manufacturer.Phone,
            Email = manufacturer.Email,
        };
    }
}
