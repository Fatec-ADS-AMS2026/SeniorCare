using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SeniorCareManager.WebAPI.Objects.Models;
using SeniorCareManager.WebAPI.Services.Entities;
using SeniorCareManager.WebAPI.Services.Interfaces;

namespace SeniorCareManager.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UnitOfMeasureController : Controller
    {
       private readonly IUnitOfMeasureService _unitOfMeasureService;

        public UnitOfMeasureController(IUnitOfMeasureService service)
        {
            this._unitOfMeasureService = service;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var unitOfMeasure = await _unitOfMeasureService.GetAll();

            if (unitOfMeasure == null) {
                return StatusCode(500, $"Nenhuma unidade de medida encontrada!");
            }

            else
            {
                return Ok(unitOfMeasure);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var unitOfMeasureId = await _unitOfMeasureService.GetById(id);
                if (unitOfMeasureId == null) return NotFound("Unidade de medida não encontrada!");
                return Ok(unitOfMeasureId);
        }

        [HttpPost]
        public async Task<IActionResult> Post(UnitOfMeasure unitOfMeasure)
        {
            if (unitOfMeasure.Description == String.Empty) return BadRequest("Unidade de medida não pode ser vazia.");
         
            if (unitOfMeasure.Abbreviation == String.Empty) return BadRequest("Abreviação não pode ser vazia.");

            try
            {
                await _unitOfMeasureService.Create(unitOfMeasure);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro ao tentar inserir uma nova unidade de medida! {ex}");
            }
            return Ok(unitOfMeasure);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UnitOfMeasure unitOfMeasure)
        {
            try
            {
                await _unitOfMeasureService.Update(unitOfMeasure, id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro ao tentar atualizar a unidade de medida: " + ex.Message);
            }

            return Ok(unitOfMeasure);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _unitOfMeasureService.Remove(id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ocorreu um erro ao tentar remover a unidade de medida! {ex}");
            }

            return Ok("Unidade de medida apagada com sucesso");
        }
    }
}
