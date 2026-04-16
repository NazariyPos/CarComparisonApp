using CarComparisonApi.Data;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// SQL-backed implementation of catalog read and search operations.
    /// </summary>
    public class CarService : ICarService
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly ILogger<CarService> _logger;

        public CarService(CarComparisonDbContext dbContext, ILogger<CarService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<CarBrand>> GetAllBrandsAsync()
        {
            return await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Trims)
                            .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CarBrand?> GetBrandByIdAsync(int id)
        {
            return await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Trims)
                            .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<GenerationWithTrimsDto?> GetGenerationWithTrimsAsync(int generationId)
        {
            var generation = await _dbContext.Generations
                .Include(g => g.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(g => g.Trims)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == generationId);

            if (generation?.Model?.Brand == null)
            {
                return null;
            }

            return new GenerationWithTrimsDto
            {
                Id = generation.Id,
                Name = generation.Name,
                YearFrom = generation.YearFrom,
                YearTo = generation.YearTo,
                PhotoUrl = generation.PhotoUrl,
                Brand = new BrandDto
                {
                    Id = generation.Model.Brand.Id,
                    Name = generation.Model.Brand.Name
                },
                Model = new ModelDto
                {
                    Id = generation.Model.Id,
                    Name = generation.Model.Name,
                    BodyType = generation.Model.BodyType ?? string.Empty,
                    BrandId = generation.Model.BrandId
                },
                Trims = generation.Trims.ConvertAll(t => new TrimBasicDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    TransmissionType = t.TransmissionType ?? string.Empty,
                    DoorsCount = t.DoorsCount,
                    SeatsCount = t.SeatsCount
                })
            };
        }

        public async Task<TrimFullDto?> GetTrimFullDetailsAsync(int trimId)
        {
            var trim = await _dbContext.Trims
                .Include(t => t.TechnicalDetails)
                .Include(t => t.Generation)
                    .ThenInclude(g => g!.Model)
                        .ThenInclude(m => m!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == trimId);

            if (trim?.Generation?.Model?.Brand == null)
            {
                return null;
            }

            return new TrimFullDto
            {
                Id = trim.Id,
                Name = trim.Name,
                TransmissionType = trim.TransmissionType ?? string.Empty,
                DoorsCount = trim.DoorsCount,
                SeatsCount = trim.SeatsCount,
                Generation = new GenerationBasicDto
                {
                    Id = trim.Generation.Id,
                    Name = trim.Generation.Name,
                    YearFrom = trim.Generation.YearFrom,
                    YearTo = trim.Generation.YearTo,
                    PhotoUrl = trim.Generation.PhotoUrl
                },
                Model = new ModelBasicDto
                {
                    Id = trim.Generation.Model!.Id,
                    Name = trim.Generation.Model.Name,
                    BodyType = trim.Generation.Model.BodyType ?? string.Empty
                },
                Brand = new BrandBasicDto
                {
                    Id = trim.Generation.Model.Brand!.Id,
                    Name = trim.Generation.Model.Brand.Name
                },
                TechnicalDetails = trim.TechnicalDetails
            };
        }

        public async Task<IEnumerable<GenerationCardDto>> GetGenerationCardsAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null)
        {
            var carData = await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Trims)
                            .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();

            var filteredBrands = carData.AsEnumerable();

            if (!string.IsNullOrEmpty(brand))
            {
                filteredBrands = filteredBrands.Where(b =>
                    b.Name.Contains(brand, StringComparison.OrdinalIgnoreCase));
            }

            var generationCards = new List<GenerationCardDto>();

            foreach (var brandItem in filteredBrands)
            {
                foreach (var modelItem in brandItem.Models)
                {
                    if (!string.IsNullOrEmpty(model) &&
                        !modelItem.Name.Contains(model, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(bodyType) &&
                        !string.Equals(modelItem.BodyType, bodyType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var genItem in modelItem.Generations)
                    {
                        if (!string.IsNullOrEmpty(generation) &&
                            !genItem.Name.Contains(generation, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (minYear.HasValue && genItem.YearFrom < minYear.Value)
                        {
                            continue;
                        }

                        if (maxYear.HasValue && genItem.YearFrom > maxYear.Value)
                        {
                            continue;
                        }

                        var filteredTrims = genItem.Trims.AsEnumerable();

                        if (!string.IsNullOrEmpty(transmission))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TransmissionType != null &&
                                string.Equals(t.TransmissionType, transmission, StringComparison.OrdinalIgnoreCase));
                        }

                        if (!string.IsNullOrEmpty(fuelType))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TechnicalDetails?.FuelType != null &&
                                string.Equals(t.TechnicalDetails.FuelType, fuelType, StringComparison.OrdinalIgnoreCase));
                        }

                        var trimsList = filteredTrims.ToList();
                        if (trimsList.Count == 0)
                        {
                            continue;
                        }

                        generationCards.Add(new GenerationCardDto
                        {
                            BrandId = brandItem.Id,
                            BrandName = brandItem.Name,
                            ModelId = modelItem.Id,
                            ModelName = modelItem.Name,
                            GenerationId = genItem.Id,
                            GenerationName = genItem.Name,
                            BodyType = modelItem.BodyType ?? string.Empty,
                            YearFrom = genItem.YearFrom,
                            YearTo = genItem.YearTo,
                            PhotoUrl = genItem.PhotoUrl,
                            TrimCount = trimsList.Count
                        });
                    }
                }
            }

            _logger.LogInformation("Total generation cards found: {Count}", generationCards.Count);
            return generationCards;
        }

        public async Task<IEnumerable<CarModel>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _dbContext.CarModels
                .Where(m => m.BrandId == brandId)
                .Include(m => m.Generations)
                    .ThenInclude(g => g.Trims)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CarModel?> GetModelByIdAsync(int id)
        {
            return await _dbContext.CarModels
                .Include(m => m.Generations)
                    .ThenInclude(g => g.Trims)
                        .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Generation>> GetGenerationsByModelIdAsync(int modelId)
        {
            return await _dbContext.Generations
                .Where(g => g.ModelId == modelId)
                .Include(g => g.Trims)
                    .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Generation?> GetGenerationByIdAsync(int id)
        {
            return await _dbContext.Generations
                .Include(g => g.Trims)
                    .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Trim>> GetTrimsByGenerationIdAsync(int generationId)
        {
            return await _dbContext.Trims
                .Where(t => t.GenerationId == generationId)
                .Include(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Trim?> GetTrimByIdAsync(int id)
        {
            return await _dbContext.Trims
                .Include(t => t.TechnicalDetails)
                .Include(t => t.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TechnicalDetails?> GetTechnicalDetailsByTrimIdAsync(int trimId)
        {
            return await _dbContext.TechnicalDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(td => td.TrimId == trimId);
        }

        public async Task<IEnumerable<CarBrand>> SearchAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null)
        {
            var data = await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Trims)
                            .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();

            var result = data.AsEnumerable();

            if (!string.IsNullOrEmpty(brand))
            {
                result = result.Where(b =>
                    b.Name.Contains(brand, StringComparison.OrdinalIgnoreCase));
            }

            var simplifiedBrands = new List<CarBrand>();

            foreach (var brandItem in result)
            {
                var filteredModels = new List<CarModel>();

                foreach (var modelItem in brandItem.Models)
                {
                    if (!string.IsNullOrEmpty(model) &&
                        !modelItem.Name.Contains(model, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(bodyType) &&
                        !string.Equals(modelItem.BodyType, bodyType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var filteredGenerations = new List<Generation>();

                    foreach (var genItem in modelItem.Generations)
                    {
                        if (!string.IsNullOrEmpty(generation) &&
                            !genItem.Name.Contains(generation, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (minYear.HasValue && genItem.YearTo < minYear.Value)
                        {
                            continue;
                        }

                        if (maxYear.HasValue && genItem.YearFrom > maxYear.Value)
                        {
                            continue;
                        }

                        var filteredTrims = genItem.Trims.AsEnumerable();

                        if (!string.IsNullOrEmpty(transmission))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TransmissionType?.Contains(transmission, StringComparison.OrdinalIgnoreCase) == true);
                        }

                        if (!string.IsNullOrEmpty(fuelType))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TechnicalDetails?.FuelType?.Contains(fuelType, StringComparison.OrdinalIgnoreCase) == true);
                        }

                        var trimsList = filteredTrims.ToList();
                        if (trimsList.Count == 0)
                        {
                            continue;
                        }

                        filteredGenerations.Add(new Generation
                        {
                            Id = genItem.Id,
                            Name = genItem.Name,
                            ModelId = genItem.ModelId,
                            YearFrom = genItem.YearFrom,
                            YearTo = genItem.YearTo,
                            PhotoUrl = genItem.PhotoUrl,
                            Trims = trimsList.ConvertAll(t => new Trim
                            {
                                Id = t.Id,
                                Name = t.Name,
                                GenerationId = t.GenerationId,
                                TransmissionType = t.TransmissionType,
                                DoorsCount = t.DoorsCount,
                                SeatsCount = t.SeatsCount,
                                TechnicalDetails = null,
                                Reviews = new List<Review>()
                            })
                        });
                    }

                    if (filteredGenerations.Count == 0)
                    {
                        continue;
                    }

                    filteredModels.Add(new CarModel
                    {
                        Id = modelItem.Id,
                        Name = modelItem.Name,
                        BrandId = modelItem.BrandId,
                        BodyType = modelItem.BodyType,
                        Generations = filteredGenerations
                    });
                }

                if (filteredModels.Count == 0)
                {
                    continue;
                }

                simplifiedBrands.Add(new CarBrand
                {
                    Id = brandItem.Id,
                    Name = brandItem.Name,
                    Models = filteredModels
                });
            }

            return simplifiedBrands;
        }

        public async Task<IEnumerable<Trim>> GetTrimsForComparisonAsync(IEnumerable<int> trimIds)
        {
            return await _dbContext.Trims
                .Where(t => trimIds.Contains(t.Id))
                .Include(t => t.TechnicalDetails)
                .Take(4)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
