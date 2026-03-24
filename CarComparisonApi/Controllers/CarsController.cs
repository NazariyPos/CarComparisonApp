using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] string? generation,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] string? bodyType,
            [FromQuery] string? transmission,
            [FromQuery] string? fuelType)
        {
            var validationErrors = new List<string>();

            if (!string.IsNullOrEmpty(model) && string.IsNullOrEmpty(brand))
            {
                validationErrors.Add("Для пошуку за моделлю необхідно вказати марку (параметр brand)");
            }

            if (!string.IsNullOrEmpty(generation))
            {
                if (string.IsNullOrEmpty(brand))
                {
                    validationErrors.Add("Для пошуку за поколінням необхідно вказати марку (параметр brand)");
                }
                if (string.IsNullOrEmpty(model))
                {
                    validationErrors.Add("Для пошуку за поколінням необхідно вказати модель (параметр model)");
                }
            }

            if (minYear.HasValue && maxYear.HasValue && minYear > maxYear)
            {
                validationErrors.Add("Мінімальний рік не може бути більшим за максимальний");
            }

            if (minYear.HasValue && minYear < 1900)
            {
                validationErrors.Add("Мінімальний рік не може бути меншим за 1900");
            }

            if (maxYear.HasValue && maxYear > DateTime.Now.Year + 1)
            {
                validationErrors.Add($"Максимальний рік не може бути більшим за {DateTime.Now.Year + 1}");
            }

            if (validationErrors.Count != 0)
            {
                return BadRequest(new
                {
                    message = "Помилки валідації параметрів пошуку",
                    errors = validationErrors
                });
            }

            try
            {
                var result = await _carService.GetGenerationCardsAsync(
                    brand, model, generation, minYear, maxYear,
                    bodyType, transmission, fuelType);

                if (!result.Any())
                {
                    return NotFound(new
                    {
                        message = "За вашими критеріями не знайдено жодного покоління авто",
                        parameters = new
                        {
                            brand,
                            model,
                            generation,
                            minYear,
                            maxYear,
                            bodyType,
                            transmission,
                            fuelType
                        }
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Сталася внутрішня помилка під час пошуку",
                    error = ex.Message
                });
            }
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetAllBrands()
        {
            var brands = await _carService.GetAllBrandsAsync();

            var brandDtos = brands.Select(b => new BrandDto
            {
                Id = b.Id,
                Name = b.Name
            }).ToList();

            return Ok(brandDtos);
        }

        [HttpGet("brands/{id}")]
        public async Task<IActionResult> GetBrandById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var brand = await _carService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound($"Марка з ID {id} не знайдена");

            return Ok(brand);
        }

        [HttpGet("brands/{brandId}/models")]
        public async Task<IActionResult> GetModelsByBrand(int brandId)
        {
            if (brandId <= 0)
            {
                return BadRequest("ID марки має бути додатним числом");
            }

            var brandExists = await _carService.GetBrandByIdAsync(brandId);
            if (brandExists == null)
            {
                return NotFound($"Марка з ID {brandId} не знайдена");
            }

            var models = await _carService.GetModelsByBrandIdAsync(brandId);

            var modelDtos = models.Select(m => new ModelDto
            {
                Id = m.Id,
                Name = m.Name,
                BodyType = m.BodyType ?? string.Empty,
                BrandId = m.BrandId
            }).ToList();

            return Ok(modelDtos);
        }

        [HttpGet("models/{id}")]
        public async Task<IActionResult> GetModelById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var model = await _carService.GetModelByIdAsync(id);
            if (model == null)
                return NotFound($"Модель з ID {id} не знайдена");

            return Ok(model);
        }

        [HttpGet("models/{modelId}/generations")]
        public async Task<IActionResult> GetGenerationsByModel(int modelId)
        {
            if (modelId <= 0)
            {
                return BadRequest("ID моделі має бути додатним числом");
            }

            var modelExists = await _carService.GetModelByIdAsync(modelId);
            if (modelExists == null)
            {
                return NotFound($"Модель з ID {modelId} не знайдена");
            }

            var generations = await _carService.GetGenerationsByModelIdAsync(modelId);

            if (!generations.Any())
            {
                return NotFound($"Для моделі з ID {modelId} не знайдено поколінь");
            }

            return Ok(generations);
        }

        [HttpGet("generations/{id}")]
        public async Task<IActionResult> GetGenerationById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var generation = await _carService.GetGenerationByIdAsync(id);
            if (generation == null)
                return NotFound($"Покоління з ID {id} не знайдена");
            return Ok(generation);
        }

        [HttpGet("generations/{generationId}/trims")]
        public async Task<IActionResult> GetTrimsByGeneration(int generationId)
        {
            if (generationId <= 0)
            {
                return BadRequest("ID покоління має бути додатним числом");
            }

            var generationExists = await _carService.GetGenerationByIdAsync(generationId);
            if (generationExists == null)
            {
                return NotFound($"Покоління з ID {generationId} не знайдена");
            }

            var trims = await _carService.GetTrimsByGenerationIdAsync(generationId);

            if (!trims.Any())
            {
                return NotFound($"Для покоління з ID {generationId} не знайдено комплектацій");
            }

            return Ok(trims);
        }

        [HttpGet("trims/{id}")]
        public async Task<IActionResult> GetTrimById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var trim = await _carService.GetTrimByIdAsync(id);
            if (trim == null)
                return NotFound($"Комплектація з ID {id} не знайдена");
            return Ok(trim);
        }

        [HttpGet("trims/{trimId}/technical-details")]
        public async Task<IActionResult> GetTechnicalDetails(int trimId)
        {
            if (trimId <= 0)
            {
                return BadRequest("ID комплектації має бути додатним числом");
            }

            var trimExists = await _carService.GetTrimByIdAsync(trimId);
            if (trimExists == null)
            {
                return NotFound($"Комплектація з ID {trimId} не знайдена");
            }

            var details = await _carService.GetTechnicalDetailsByTrimIdAsync(trimId);
            if (details == null)
                return NotFound($"Технічні характеристики для комплектації {trimId} не знайдені");
            return Ok(details);
        }

        [HttpGet("generations/{id}/details")]
        public async Task<IActionResult> GetGenerationDetails(int id)
        {
            if (id <= 0)
                return BadRequest("ID має бути додатним числом");

            var generation = await _carService.GetGenerationWithTrimsAsync(id);
            if (generation == null)
                return NotFound($"Покоління з ID {id} не знайдено");

            return Ok(generation);
        }

        [HttpGet("trims/{id}/full")]
        public async Task<IActionResult> GetTrimFullDetails(int id)
        {
            if (id <= 0)
                return BadRequest("ID має бути додатним числом");

            var trim = await _carService.GetTrimFullDetailsAsync(id);
            if (trim == null)
                return NotFound($"Комплектація з ID {id} не знайдена");

            return Ok(trim);
        }

    }
}
