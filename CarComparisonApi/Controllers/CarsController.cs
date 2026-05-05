using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Provides endpoints for searching and retrieving cars, generations and trims.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        /// <summary>
        /// Searches generation cards by brand/model and optional filters.
        /// </summary>
        /// <param name="brand">Brand name filter.</param>
        /// <param name="model">Model name filter.</param>
        /// <param name="generation">Generation name filter.</param>
        /// <param name="minYear">Minimum production year.</param>
        /// <param name="maxYear">Maximum production year.</param>
        /// <param name="bodyStyleId">Body style identifier filter.</param>
        /// <param name="variantType">Variant type filter (deprecated, use generationVariantId).</param>
        /// <param name="transmission">Transmission type filter.</param>
        /// <param name="fuelType">Fuel type filter.</param>
        /// <param name="brandId">Optional brand identifier filter.</param>
        /// <param name="modelId">Optional model identifier filter.</param>
        /// <param name="generationId">Optional generation identifier filter (deprecated, use generationVariantId).</param>
        /// <param name="generationVariantId">Optional generation variant identifier filter.</param>
        /// <returns>Filtered generation cards or validation/result errors.</returns>
        [HttpGet("search")]
        [SwaggerOperation(Summary = "Search car generations")]
        [ProducesResponseType(typeof(IEnumerable<GenerationCardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search(
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] string? generation,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] int? bodyStyleId,
            [FromQuery] string? variantType,
            [FromQuery] string? transmission,
            [FromQuery] string? fuelType,
            [FromQuery] int? brandId,
            [FromQuery] int? modelId,
            [FromQuery] int? generationId,
            [FromQuery] int? generationVariantId)
        {
            var validationErrors = new List<string>();
            GenerationVariantType? parsedVariantType = null;

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

            if (bodyStyleId <= 0)
            {
                validationErrors.Add("ID кузова має бути додатним числом");
            }

            if (!string.IsNullOrWhiteSpace(variantType))
            {
                if (Enum.TryParse<GenerationVariantType>(variantType, true, out var parsed))
                {
                    parsedVariantType = parsed;
                }
                else
                {
                    validationErrors.Add("Невідомий тип рестайлінгу (variantType)");
                }
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
                    bodyStyleId, parsedVariantType, transmission, fuelType,
                    brandId, modelId, generationId, generationVariantId);

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
                            bodyStyleId,
                            variantType,
                            transmission,
                            fuelType,
                            brandId,
                            modelId,
                            generationId
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

        /// <summary>
        /// Returns available filter options for current search scope.
        /// </summary>
        /// <param name="brand">Brand name filter.</param>
        /// <param name="model">Model name filter.</param>
        /// <param name="generation">Generation name filter.</param>
        /// <param name="minYear">Minimum production year.</param>
        /// <param name="maxYear">Maximum production year.</param>
        /// <returns>Aggregated facet values for body type, transmission and fuel.</returns>
        [HttpGet("search-facets")]
        [SwaggerOperation(Summary = "Get available search filter options")]
        [ProducesResponseType(typeof(SearchFacetsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchFacets(
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] string? generation,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear)
        {
            try
            {
                var facets = await _carService.GetSearchFacetsAsync(
                    brand,
                    model,
                    generation,
                    minYear,
                    maxYear);

                return Ok(facets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Сталася внутрішня помилка під час отримання фільтрів",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Returns a list of all car brands.
        /// </summary>
        /// <returns>List of car brands.</returns>
        [HttpGet("brands")]
        [SwaggerOperation(Summary = "Get all car brands")]
        [ProducesResponseType(typeof(IEnumerable<BrandDto>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Returns brand details by identifier.
        /// </summary>
        /// <param name="id">Brand identifier.</param>
        /// <returns>Brand details.</returns>
        [HttpGet("brands/{id}")]
        [SwaggerOperation(Summary = "Get brand by ID")]
        [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns models by brand identifier.
        /// </summary>
        /// <param name="brandId">Brand identifier.</param>
        /// <returns>List of models for the brand.</returns>
        [HttpGet("brands/{brandId}/models")]
        [SwaggerOperation(Summary = "Get models by brand ID")]
        [ProducesResponseType(typeof(IEnumerable<ModelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns model details by identifier.
        /// </summary>
        /// <param name="id">Model identifier.</param>
        /// <returns>Model details.</returns>
        [HttpGet("models/{id}")]
        [SwaggerOperation(Summary = "Get model by ID")]
        [ProducesResponseType(typeof(ModelDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns generations by model identifier.
        /// </summary>
        /// <param name="modelId">Model identifier.</param>
        /// <returns>List of generations for the model.</returns>
        [HttpGet("models/{modelId}/generations")]
        [SwaggerOperation(Summary = "Get generations by model ID")]
        [ProducesResponseType(typeof(IEnumerable<Generation>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns generation variants by model identifier.
        /// </summary>
        /// <param name="modelId">Model identifier.</param>
        /// <returns>List of generation variants for the model.</returns>
        [HttpGet("models/{modelId}/variants")]
        [SwaggerOperation(Summary = "Get generation variants by model ID")]
        [ProducesResponseType(typeof(IEnumerable<GenerationVariantDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGenerationVariantsByModel(int modelId)
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

            var variants = await _carService.GetGenerationVariantsByModelIdAsync(modelId);

            if (!variants.Any())
            {
                return NotFound($"Для моделі з ID {modelId} не знайдено варіантів поколінь");
            }

            return Ok(variants);
        }

        /// <summary>
        /// Returns generation details by identifier.
        /// </summary>
        /// <param name="id">Generation identifier.</param>
        /// <returns>Generation details.</returns>
        [HttpGet("generations/{id}")]
        [SwaggerOperation(Summary = "Get generation by ID")]
        [ProducesResponseType(typeof(Generation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns trims by generation identifier.
        /// </summary>
        /// <param name="generationId">Generation identifier.</param>
        /// <returns>List of trims for the generation.</returns>
        [HttpGet("generations/{generationId}/trims")]
        [SwaggerOperation(Summary = "Get trims by generation ID")]
        [ProducesResponseType(typeof(IEnumerable<Trim>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns trims by generation variant identifier.
        /// </summary>
        /// <param name="variantId">Generation variant identifier.</param>
        /// <returns>List of trims for the variant.</returns>
        [HttpGet("variants/{variantId}/trims")]
        [SwaggerOperation(Summary = "Get trims by generation variant ID")]
        [ProducesResponseType(typeof(IEnumerable<Trim>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrimsByGenerationVariant(int variantId)
        {
            if (variantId <= 0)
            {
                return BadRequest("ID варіанту покоління має бути додатним числом");
            }

            var trims = await _carService.GetTrimsByGenerationVariantIdAsync(variantId);

            if (!trims.Any())
            {
                return NotFound($"Для варіанту покоління з ID {variantId} не знайдено комплектацій");
            }

            return Ok(trims);
        }

        /// <summary>
        /// Returns trim details by identifier.
        /// </summary>
        /// <param name="id">Trim identifier.</param>
        /// <returns>Trim details.</returns>
        [HttpGet("trims/{id}")]
        [SwaggerOperation(Summary = "Get trim by ID")]
        [ProducesResponseType(typeof(Trim), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns technical details for a specific trim.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Technical details of the trim.</returns>
        [HttpGet("trims/{trimId}/technical-details")]
        [SwaggerOperation(Summary = "Get technical details by trim ID")]
        [ProducesResponseType(typeof(TechnicalDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Returns generation details with its trims.
        /// </summary>
        /// <param name="id">Generation identifier.</param>
        /// <returns>Generation details with trims.</returns>
        [HttpGet("generations/{id}/details")]
        [SwaggerOperation(Summary = "Get generation details with trims")]
        [ProducesResponseType(typeof(GenerationWithTrimsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGenerationDetails(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var generationDetails = await _carService.GetGenerationWithTrimsAsync(id);
            if (generationDetails == null)
                return NotFound($"Покоління з ID {id} не знайдена");

            return Ok(generationDetails);
        }

        /// <summary>
        /// Returns variant-first generation details with trims.
        /// </summary>
        /// <param name="id">Generation variant identifier.</param>
        /// <returns>Generation details projected to the requested variant.</returns>
        [HttpGet("variants/{id}/details")]
        [SwaggerOperation(Summary = "Get generation variant details with trims")]
        [ProducesResponseType(typeof(GenerationWithTrimsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGenerationVariantDetails(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var variantDetails = await _carService.GetGenerationVariantWithTrimsAsync(id);
            if (variantDetails == null)
                return NotFound($"Варіант покоління з ID {id} не знайдений");

            return Ok(variantDetails);
        }

        /// <summary>
        /// Returns full trim details with hierarchy and specs.
        /// </summary>
        /// <param name="id">Trim identifier.</param>
        /// <returns>Full trim details.</returns>
        [HttpGet("trims/{id}/details")]
        [SwaggerOperation(Summary = "Get full trim details")]
        [ProducesResponseType(typeof(TrimFullDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrimDetails(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID має бути додатним числом");
            }

            var trimDetails = await _carService.GetTrimFullDetailsAsync(id);
            if (trimDetails == null)
                return NotFound($"Комплектація з ID {id} не знайдена");

            return Ok(trimDetails);
        }

        /// <summary>
        /// Returns trims for comparison.
        /// </summary>
        /// <param name="trimIds">Trim identifiers to compare.</param>
        /// <returns>Trims for comparison.</returns>
        [HttpPost("trims/comparison")]
        [SwaggerOperation(Summary = "Get trims for comparison")]
        [ProducesResponseType(typeof(IEnumerable<Trim>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTrimsForComparison([FromBody] List<int> trimIds)
        {
            if (trimIds == null || trimIds.Count == 0)
            {
                return BadRequest("Потрібно вказати хоча б один ID комплектації");
            }

            var trims = await _carService.GetTrimsForComparisonAsync(trimIds);
            return Ok(trims);
        }
    }
}
