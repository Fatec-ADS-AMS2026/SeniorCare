using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;

        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var suppliers = await _supplierService.GetAll();
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _supplierService.GetById(id);
            if (supplier == null) return NotFound("Fornecedor não encontrado!");
            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Supplier supplier)
        {
            try
            {
                await _supplierService.Create(supplier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo fornecedor: " + ex.Message);
            }
            return Ok(supplier);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Supplier supplier)
        {
            try
            {
                await _supplierService.Update(supplier, id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar atualizar o fornecedor: " + ex.Message);
            }
            return Ok(supplier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _supplierService.Remove(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar remover o fornecedor: " + ex.Message);
            }
            return Ok("Fornecedor removido com sucesso");
        }
    }
}
