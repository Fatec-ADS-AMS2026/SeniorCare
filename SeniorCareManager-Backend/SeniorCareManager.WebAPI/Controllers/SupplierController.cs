using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Dtos;
using SeniorCareManager.WebAPI.Objects.Dtos.Common;
using SeniorCareManager.WebAPI.Objects.Dtos.Requests;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<SupplierDTO>>> Get([FromQuery] CatalogQuery query)
        {
            var suppliers = await _supplierService.GetAll();
            var filtered = string.IsNullOrWhiteSpace(query.Search)
                ? suppliers
                : suppliers.Where(s =>
                    s.CorporateName.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                    s.TradeName.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            return Ok(filtered.Select(ToDto).ToPagedResult(query.Page, query.PageSize));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDTO>> GetById(int id)
        {
            var supplier = await _supplierService.GetById(id);
            if (supplier == null) throw new KeyNotFoundException("Fornecedor não encontrado.");
            return Ok(ToDto(supplier));
        }

        [HttpPost]
        public async Task<ActionResult<SupplierDTO>> Post(SupplierCreateRequest request)
        {
            var supplier = ToModel(0, request);
            await _supplierService.Create(supplier);
            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, ToDto(supplier));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<SupplierDTO>> Put(int id, SupplierUpdateRequest request)
        {
            var supplier = ToModel(id, request);
            await _supplierService.Update(supplier, id);
            return Ok(ToDto(supplier));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _supplierService.Remove(id);
            return NoContent();
        }

        private static Supplier ToModel(int id, SupplierCreateRequest request) => new(
            id, request.CorporateName, request.TradeName, request.CpfCnpj, request.Email, request.Phone,
            request.PostalCode, request.Street, request.Number, request.District, request.AddressComplement,
            request.City, request.State);

        private static SupplierDTO ToDto(Supplier supplier) => new()
        {
            Id = supplier.Id,
            CorporateName = supplier.CorporateName,
            TradeName = supplier.TradeName,
            CpfCnpj = supplier.CpfCnpj,
            Email = supplier.Email,
            Phone = supplier.Phone,
            PostalCode = supplier.PostalCode,
            Street = supplier.Street,
            Number = supplier.Number,
            District = supplier.District,
            AddressComplement = supplier.AddressComplement,
            City = supplier.City,
            State = supplier.State,
        };
    }
}
