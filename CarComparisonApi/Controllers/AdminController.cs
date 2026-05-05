using System.Security.Claims;
using CarComparisonApi.Data;
using CarComparisonApi.Models;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly IJsonUserService _userService;

        public AdminController(CarComparisonDbContext dbContext, IJsonUserService userService)
        {
            _dbContext = dbContext;
            _userService = userService;
        }

        private async Task<bool> IsCurrentUserAdminAsync()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out var userId)) return false;
            var user = await _userService.GetUserByIdAsync(userId);
            return user?.IsAdmin ?? false;
        }

        public record CreateBrandRequest(string Name);
        public record CreateModelRequest(string Name, string? BodyType);
        public record CreateGenerationRequest(string Name, int YearFrom, int YearTo, string? PhotoUrl);
        public record CreateVariantRequest(string Name, string VariantType, int? BodyStyleId, int DoorsCount, int YearFrom, int YearTo, bool IsDefault, string? PhotoUrl);
        public record CreateTrimRequest(string Name, string? TransmissionType, int? DoorsCount, int? SeatsCount);
        public record CreateTechnicalDetailsRequest(string? FuelType, int? Power, string? DriveType);

        [HttpPost("brands")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            var normalizedName = req.Name.Trim();
            var existingBrand = await _dbContext.CarBrands
                .FirstOrDefaultAsync(brand => brand.Name == normalizedName);

            if (existingBrand != null)
            {
                return Conflict(new { message = "Brand already exists", id = existingBrand.Id, name = existingBrand.Name });
            }

            var brand = new CarBrand { Name = normalizedName };
            await _dbContext.CarBrands.AddAsync(brand);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = brand.Id, name = brand.Name });
        }

        [HttpPost("brands/{brandId}/models")]
        public async Task<IActionResult> CreateModel(int brandId, [FromBody] CreateModelRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (brandId <= 0) return BadRequest("Invalid brandId");
            var brand = await _dbContext.CarBrands.FindAsync(brandId);
            if (brand == null) return NotFound("Brand not found");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            var normalizedName = req.Name.Trim();
            var existingModel = await _dbContext.CarModels
                .FirstOrDefaultAsync(model => model.BrandId == brandId && model.Name == normalizedName);

            if (existingModel != null)
            {
                return Conflict(new { message = "Model already exists for this brand", id = existingModel.Id, name = existingModel.Name, brandId });
            }

            var model = new CarModel { Name = normalizedName, BodyType = req.BodyType?.Trim(), BrandId = brandId };
            await _dbContext.CarModels.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = model.Id, name = model.Name, brandId = brandId });
        }

        [HttpPost("models/{modelId}/generations")]
        public async Task<IActionResult> CreateGeneration(int modelId, [FromBody] CreateGenerationRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (modelId <= 0) return BadRequest("Invalid modelId");
            var model = await _dbContext.CarModels.FindAsync(modelId);
            if (model == null) return NotFound("Model not found");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            var normalizedName = req.Name.Trim();
            var existingGeneration = await _dbContext.Generations
                .FirstOrDefaultAsync(generation => generation.ModelId == modelId && generation.Name == normalizedName);

            if (existingGeneration != null)
            {
                return Conflict(new { message = "Generation already exists for this model", id = existingGeneration.Id, name = existingGeneration.Name, modelId });
            }

            var generation = new Generation { Name = normalizedName, ModelId = modelId, YearFrom = req.YearFrom, YearTo = req.YearTo, PhotoUrl = req.PhotoUrl };
            await _dbContext.Generations.AddAsync(generation);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = generation.Id, name = generation.Name, modelId = modelId });
        }

        [HttpPost("generations/{generationId}/variants")]
        public async Task<IActionResult> CreateGenerationVariant(int generationId, [FromBody] CreateVariantRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (generationId <= 0) return BadRequest("Invalid generationId");
            var generation = await _dbContext.Generations.FindAsync(generationId);
            if (generation == null) return NotFound("Generation not found");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            if (!Enum.TryParse<GenerationVariantType>(req.VariantType, true, out var variantType))
            {
                variantType = GenerationVariantType.Standard;
            }

            if (!req.BodyStyleId.HasValue || req.BodyStyleId <= 0)
            {
                return BadRequest("BodyStyleId is required and must reference an existing body style");
            }

            var normalizedName = req.Name.Trim();
            var existingVariant = await _dbContext.GenerationVariants
                .FirstOrDefaultAsync(variant =>
                    variant.GenerationId == generationId &&
                    variant.Name == normalizedName &&
                    variant.VariantType == variantType &&
                    variant.BodyStyleId == req.BodyStyleId.Value &&
                    variant.DoorsCount == req.DoorsCount);

            if (existingVariant != null)
            {
                return Conflict(new { message = "Variant already exists for this generation", id = existingVariant.Id, name = existingVariant.Name, generationId });
            }

            var variant = new GenerationVariant
            {
                Name = normalizedName,
                GenerationId = generationId,
                VariantType = variantType,
                BodyStyleId = req.BodyStyleId.Value,
                DoorsCount = req.DoorsCount,
                YearFrom = req.YearFrom,
                YearTo = req.YearTo,
                IsDefault = req.IsDefault,
                PhotoUrl = req.PhotoUrl
            };

            await _dbContext.GenerationVariants.AddAsync(variant);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = variant.Id, name = variant.Name, generationId = generationId });
        }

        [HttpPost("variants/{variantId}/trims")]
        public async Task<IActionResult> CreateTrim(int variantId, [FromBody] CreateTrimRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (variantId <= 0) return BadRequest("Invalid variantId");
            var variant = await _dbContext.GenerationVariants.FindAsync(variantId);
            if (variant == null) return NotFound("Variant not found");
            if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required");

            var normalizedName = req.Name.Trim();
            var normalizedTransmission = req.TransmissionType?.Trim();
            var existingTrim = await _dbContext.Trims
                .FirstOrDefaultAsync(trim =>
                    trim.GenerationVariantId == variantId &&
                    trim.Name == normalizedName &&
                    trim.TransmissionType == normalizedTransmission);

            if (existingTrim != null)
            {
                return Conflict(new { message = "Trim already exists for this variant", id = existingTrim.Id, name = existingTrim.Name, variantId });
            }

            var trim = new Trim
            {
                Name = normalizedName,
                GenerationVariantId = variantId,
                TransmissionType = normalizedTransmission,
                DoorsCount = req.DoorsCount,
                SeatsCount = req.SeatsCount
            };

            await _dbContext.Trims.AddAsync(trim);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = trim.Id, name = trim.Name, variantId = variantId });
        }

        [HttpPost("trims/{trimId}/technical-details")]
        public async Task<IActionResult> CreateTechnicalDetails(int trimId, [FromBody] CreateTechnicalDetailsRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (trimId <= 0) return BadRequest("Invalid trimId");
            var trim = await _dbContext.Trims.FindAsync(trimId);
            if (trim == null) return NotFound("Trim not found");

            var existingDetails = await _dbContext.TechnicalDetails
                .FirstOrDefaultAsync(details => details.TrimId == trimId);

            if (existingDetails != null)
            {
                existingDetails.FuelType = req.FuelType?.Trim();
                existingDetails.Power = req.Power;
                existingDetails.DriveType = req.DriveType?.Trim();

                await _dbContext.SaveChangesAsync();
                return Ok(new { id = existingDetails.Id, trimId });
            }

            var details = new TechnicalDetails
            {
                TrimId = trimId,
                FuelType = req.FuelType?.Trim(),
                Power = req.Power,
                DriveType = req.DriveType?.Trim()
            };

            await _dbContext.TechnicalDetails.AddAsync(details);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = details.Id, trimId = trimId });
        }
    }
}
