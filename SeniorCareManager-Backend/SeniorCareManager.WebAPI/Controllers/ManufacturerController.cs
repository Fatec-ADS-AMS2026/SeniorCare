using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Get()
        {
            var manufacturers = await _manufacturerService.GetAll();
            return Ok(manufacturers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _manufacturerService.GetById(id);
            if (manufacturer == null) return NotFound("Fabricante não encontrado!");
            return Ok(manufacturer);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Manufacturer manufacturer)
        {
            try
            {
                await _manufacturerService.Create(manufacturer);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar inserir um novo fabricante.");
            }
            return CreatedAtAction(nameof(GetById), new { id = manufacturer.Id }, manufacturer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, Manufacturer manufacturer)
        {
            try
            {
                await _manufacturerService.Update(manufacturer, id);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar atualizar o fabricante.");
            }
            return Ok(manufacturer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _manufacturerService.Remove(id);
                return Ok("Fabricante apagado com sucesso");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar remover o fabricante.");
            }
        }
    }
}