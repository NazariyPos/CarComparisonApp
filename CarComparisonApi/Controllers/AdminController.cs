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
        public record CreateTechnicalDetailsRequest(
            int? MaxSpeed,
            decimal? Acceleration0To100,
            string? EngineCode,
            string? EngineType,
            int? CylindersCount,
            int? ValvesCount,
            decimal? CompressionRatio,
            string? FuelType,
            int? Power,
            int? Torque,
            int? MaxPowerAtRPM,
            int? MaxTorqueAtRPM,
            decimal? EngineDisplacement,
            string? DriveType,
            decimal? FuelConsumptionCity,
            decimal? FuelConsumptionMixed,
            decimal? FuelConsumptionHighway,
            decimal? ElectricRange,
            decimal? Length,
            decimal? Width,
            decimal? Height,
            decimal? Wheelbase,
            decimal? FrontTrack,
            decimal? RearTrack,
            decimal? CurbWeight,
            decimal? GrossWeight,
            decimal? FuelTankCapacity,
            decimal? TurningCircle,
            string? FrontBrakes,
            string? RearBrakes,
            string? FrontSuspension,
            string? RearSuspension
        );

        // Дозволені значення для контролю якості даних
        private static readonly HashSet<string> AllowedBodyTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Седан", "Купе", "Універсал", "Хетчбек", "Позашляховик", "Мінівен", "Кабріолет"
        };

        private static readonly HashSet<string> AllowedFuelTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Бензин", "Дизель", "Гібрид", "Електро", "LPG"
        };

        private static readonly HashSet<string> AllowedDriveTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "FWD", "RWD", "AWD", "4WD"
        };

        [HttpGet("body-styles")]
        public async Task<IActionResult> GetBodyStyles()
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            
            var bodyStyles = await _dbContext.BodyStyles
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Select(b => new { id = b.Id, name = b.Name })
                .ToListAsync();

            return Ok(bodyStyles);
        }

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

            // Валідація типу кузова
            if (!string.IsNullOrWhiteSpace(req.BodyType) && !AllowedBodyTypes.Contains(req.BodyType))
            {
                return BadRequest($"Invalid body type. Allowed values: {string.Join(", ", AllowedBodyTypes)}");
            }

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

        [HttpPost("models/{modelId}/variants")]
        public async Task<IActionResult> CreateGenerationVariant(int modelId, [FromBody] CreateVariantRequest req)
        {
            if (!await IsCurrentUserAdminAsync()) return Forbid();
            if (modelId <= 0) return BadRequest("Invalid modelId");
            var model = await _dbContext.CarModels.FindAsync(modelId);
            if (model == null) return NotFound("Model not found");
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
                    variant.ModelId == modelId &&
                    variant.Name == normalizedName &&
                    variant.VariantType == variantType &&
                    variant.BodyStyleId == req.BodyStyleId.Value &&
                    variant.DoorsCount == req.DoorsCount);

            if (existingVariant != null)
            {
                return Conflict(new { message = "Variant already exists for this model", id = existingVariant.Id, name = existingVariant.Name, modelId });
            }

            var variant = new GenerationVariant
            {
                Name = normalizedName,
                ModelId = modelId,
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

            return Ok(new { id = variant.Id, name = variant.Name, modelId = modelId });
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

            // Валідація типу палива
            if (!string.IsNullOrWhiteSpace(req.FuelType) && !AllowedFuelTypes.Contains(req.FuelType))
            {
                return BadRequest($"Invalid fuel type. Allowed values: {string.Join(", ", AllowedFuelTypes)}");
            }

            // Валідація приводу
            if (!string.IsNullOrWhiteSpace(req.DriveType) && !AllowedDriveTypes.Contains(req.DriveType))
            {
                return BadRequest($"Invalid drive type. Allowed values: {string.Join(", ", AllowedDriveTypes)}");
            }

            var existingDetails = await _dbContext.TechnicalDetails
                .FirstOrDefaultAsync(details => details.TrimId == trimId);

            if (existingDetails != null)
            {
                existingDetails.MaxSpeed = req.MaxSpeed;
                existingDetails.Acceleration0To100 = req.Acceleration0To100;
                existingDetails.EngineCode = req.EngineCode?.Trim();
                existingDetails.EngineType = req.EngineType?.Trim();
                existingDetails.CylindersCount = req.CylindersCount;
                existingDetails.ValvesCount = req.ValvesCount;
                existingDetails.CompressionRatio = req.CompressionRatio;
                existingDetails.FuelType = req.FuelType?.Trim();
                existingDetails.Power = req.Power;
                existingDetails.Torque = req.Torque;
                existingDetails.MaxPowerAtRPM = req.MaxPowerAtRPM;
                existingDetails.MaxTorqueAtRPM = req.MaxTorqueAtRPM;
                existingDetails.EngineDisplacement = req.EngineDisplacement;
                existingDetails.DriveType = req.DriveType?.Trim();
                existingDetails.FuelConsumptionCity = req.FuelConsumptionCity;
                existingDetails.FuelConsumptionMixed = req.FuelConsumptionMixed;
                existingDetails.FuelConsumptionHighway = req.FuelConsumptionHighway;
                existingDetails.ElectricRange = req.ElectricRange;
                existingDetails.Length = req.Length;
                existingDetails.Width = req.Width;
                existingDetails.Height = req.Height;
                existingDetails.Wheelbase = req.Wheelbase;
                existingDetails.FrontTrack = req.FrontTrack;
                existingDetails.RearTrack = req.RearTrack;
                existingDetails.CurbWeight = req.CurbWeight;
                existingDetails.GrossWeight = req.GrossWeight;
                existingDetails.FuelTankCapacity = req.FuelTankCapacity;
                existingDetails.TurningCircle = req.TurningCircle;
                existingDetails.FrontBrakes = req.FrontBrakes?.Trim();
                existingDetails.RearBrakes = req.RearBrakes?.Trim();
                existingDetails.FrontSuspension = req.FrontSuspension?.Trim();
                existingDetails.RearSuspension = req.RearSuspension?.Trim();

                await _dbContext.SaveChangesAsync();
                return Ok(new { id = existingDetails.Id, trimId });
            }

            var details = new TechnicalDetails
            {
                TrimId = trimId,
                MaxSpeed = req.MaxSpeed,
                Acceleration0To100 = req.Acceleration0To100,
                EngineCode = req.EngineCode?.Trim(),
                EngineType = req.EngineType?.Trim(),
                CylindersCount = req.CylindersCount,
                ValvesCount = req.ValvesCount,
                CompressionRatio = req.CompressionRatio,
                FuelType = req.FuelType?.Trim(),
                Power = req.Power,
                Torque = req.Torque,
                MaxPowerAtRPM = req.MaxPowerAtRPM,
                MaxTorqueAtRPM = req.MaxTorqueAtRPM,
                EngineDisplacement = req.EngineDisplacement,
                DriveType = req.DriveType?.Trim(),
                FuelConsumptionCity = req.FuelConsumptionCity,
                FuelConsumptionMixed = req.FuelConsumptionMixed,
                FuelConsumptionHighway = req.FuelConsumptionHighway,
                ElectricRange = req.ElectricRange,
                Length = req.Length,
                Width = req.Width,
                Height = req.Height,
                Wheelbase = req.Wheelbase,
                FrontTrack = req.FrontTrack,
                RearTrack = req.RearTrack,
                CurbWeight = req.CurbWeight,
                GrossWeight = req.GrossWeight,
                FuelTankCapacity = req.FuelTankCapacity,
                TurningCircle = req.TurningCircle,
                FrontBrakes = req.FrontBrakes?.Trim(),
                RearBrakes = req.RearBrakes?.Trim(),
                FrontSuspension = req.FrontSuspension?.Trim(),
                RearSuspension = req.RearSuspension?.Trim()
            };

            await _dbContext.TechnicalDetails.AddAsync(details);
            await _dbContext.SaveChangesAsync();

            return Ok(new { id = details.Id, trimId = trimId });
        }
    }
}
