using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CarrierController : Controller
    {

        private readonly ICarrierService _carrierService;

        public CarrierController(ICarrierService carrierService)
        {
            this._carrierService = carrierService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var carriers = await _carrierService.GetAll();
            return Ok(carriers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var carriers = await _carrierService.GetById(id);
            if (carriers == null)
                return NotFound("Transportadora não encontrada");
            return Ok(carriers);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Carrier carrier)
        {
            try
            {
                await _carrierService.Create(carrier);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar inserir uma nova transportadora");
            }
            return Ok(carrier);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Carrier carrier)
        {
            try
            {
                await _carrierService.Update(carrier, id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar atualizar os dados da transportadora" + ex.Message);
            }
            return Ok(carrier);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _carrierService.Remove(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar remover uma transportadora.");
            }
            return Ok("Transportadora removida com suceso");
        }
    }
}
