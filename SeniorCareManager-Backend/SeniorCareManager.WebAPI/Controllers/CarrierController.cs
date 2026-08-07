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
    public class CarrierController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarrierController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<CarrierDTO>>> GetAll([FromQuery] CatalogQuery query)
        {
            var carriers = await _carrierService.GetAll();
            var filtered = string.IsNullOrWhiteSpace(query.Search)
                ? carriers
                : carriers.Where(c =>
                    c.CorporateName.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                    c.TradeName.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CarrierDTO>> GetById(int id)
        {
            var carrier = await _carrierService.GetById(id);
            if (carrier == null) throw new KeyNotFoundException("Transportadora não encontrada.");
            return Ok(ToDto(carrier));
        }

        [HttpPost]
        public async Task<ActionResult<CarrierDTO>> Post(CarrierCreateRequest request)
        {
            var carrier = ToModel(0, request.CorporateName, request.TradeName, request.CpfCnpj, request);
            await _carrierService.Create(carrier);
            return CreatedAtAction(nameof(GetById), new { id = carrier.Id }, ToDto(carrier));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CarrierDTO>> Put(int id, CarrierUpdateRequest request)
        {
            var carrier = ToModel(id, request.CorporateName, request.TradeName, request.CpfCnpj, request);
            await _carrierService.Update(carrier, id);
            return Ok(ToDto(carrier));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _carrierService.Remove(id);
            return NoContent();
        }

        private static Carrier ToModel(int id, string corporateName, string tradeName, string cpfCnpj, CarrierCreateRequest request) => new(
            id, corporateName, tradeName, cpfCnpj,
            request.Street, request.Number, request.District, request.AddressComplement,
            request.City, request.State, request.PostalCode, request.Phone, request.Email);

        private static CarrierDTO ToDto(Carrier carrier) => new()
        {
            Id = carrier.Id,
            CorporateName = carrier.CorporateName,
            TradeName = carrier.TradeName,
            CpfCnpj = carrier.CpfCnpj,
            Street = carrier.Street,
            Number = carrier.Number,
            District = carrier.District,
            AddressComplement = carrier.AddressComplement,
            City = carrier.City,
            State = carrier.State,
            PostalCode = carrier.PostalCode,
            Phone = carrier.Phone,
            Email = carrier.Email,
        };
    }
}
