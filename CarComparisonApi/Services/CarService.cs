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
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Images)
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Trims)
                                .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CarBrand?> GetBrandByIdAsync(int id)
        {
            return await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Images)
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Trims)
                                .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<GenerationWithTrimsDto?> GetGenerationWithTrimsAsync(int generationId)
        {
            var generation = await _dbContext.Generations
                .Include(g => g.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.Images)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.BodyStyle)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.Trims)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == generationId);

            if (generation?.Model?.Brand == null)
            {
                return null;
            }

            var defaultVariant = generation.Variants
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.Id)
                .FirstOrDefault();

            return new GenerationWithTrimsDto
            {
                Id = generation.Id,
                GenerationVariantId = defaultVariant?.Id ?? 0,
                LegacyGenerationId = generation.Id,
                Name = generation.Name,
                DisplayName = defaultVariant?.Name ?? generation.Name,
                YearFrom = generation.YearFrom,
                YearTo = generation.YearTo,
                PhotoUrl = generation.Variants
                    .OrderByDescending(v => v.IsDefault)
                    .Select(v => v.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? v.PhotoUrl)
                    .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
                    ?? generation.PhotoUrl,
                Variants = generation.Variants
                    .OrderBy(v => v.YearFrom)
                    .ThenBy(v => v.BodyStyle!.Name)
                    .Select(v => new GenerationVariantDto
                    {
                        Id = v.Id,
                        Name = v.Name,
                        VariantType = v.VariantType.ToString(),
                        BodyStyleId = v.BodyStyleId,
                        BodyStyleName = v.BodyStyle?.Name ?? string.Empty,
                        DoorsCount = v.DoorsCount,
                        YearFrom = v.YearFrom,
                        YearTo = v.YearTo,
                        IsDefault = v.IsDefault,
                        PhotoUrl = v.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? v.PhotoUrl,
                        Images = v.Images
                            .OrderByDescending(i => i.IsPrimary)
                            .ThenBy(i => i.SortOrder)
                            .ThenBy(i => i.Id)
                            .Select(i => new GenerationImageDto
                            {
                                Id = i.Id,
                                GenerationVariantId = i.GenerationVariantId,
                                Url = i.Url,
                                IsPrimary = i.IsPrimary,
                                SortOrder = i.SortOrder,
                                CreatedAt = i.CreatedAt
                            })
                            .ToList()
                    })
                    .ToList(),
                Brand = new BrandDto
                {
                    Id = generation.Model.Brand.Id,
                    Name = generation.Model.Brand.Name
                },
                Model = new ModelDto
                {
                    Id = generation.Model.Id,
                    Name = generation.Model.Name,
                    BodyType = string.Join(
                        " / ",
                        generation.Variants
                            .Select(v => v.BodyStyle?.Name)
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Distinct()
                            .Order()),
                    BrandId = generation.Model.BrandId
                },
                Trims = generation.Variants
                    .SelectMany(v => v.Trims.Select(t => new TrimBasicDto
                    {
                        Id = t.Id,
                        GenerationVariantId = v.Id,
                        Name = t.Name,
                        TransmissionType = t.TransmissionType ?? string.Empty,
                        DoorsCount = t.DoorsCount,
                        SeatsCount = t.SeatsCount,
                        VariantType = v.VariantType.ToString(),
                        BodyStyleName = v.BodyStyle?.Name ?? string.Empty
                    }))
                    .OrderBy(t => t.GenerationVariantId)
                    .ThenBy(t => t.Name)
                    .ToList()
            };
        }

        public async Task<GenerationWithTrimsDto?> GetGenerationVariantWithTrimsAsync(int generationVariantId)
        {
            var variant = await _dbContext.GenerationVariants
                .Include(v => v.Generation)
                    .ThenInclude(g => g!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(v => v.Images)
                .Include(v => v.BodyStyle)
                .Include(v => v.Trims)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == generationVariantId);

            if (variant?.Generation?.Model?.Brand == null)
            {
                return null;
            }

            var generation = variant.Generation;

            return new GenerationWithTrimsDto
            {
                Id = variant.Id,
                GenerationVariantId = variant.Id,
                LegacyGenerationId = generation.Id,
                Name = variant.Name,
                DisplayName = variant.Name,
                YearFrom = variant.YearFrom,
                YearTo = variant.YearTo,
                PhotoUrl = variant.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? variant.PhotoUrl ?? generation.PhotoUrl,
                Variants = new List<GenerationVariantDto>
                {
                    new()
                    {
                        Id = variant.Id,
                        GenerationId = variant.GenerationId,
                        Name = variant.Name,
                        VariantType = variant.VariantType.ToString(),
                        BodyStyleId = variant.BodyStyleId,
                        BodyStyleName = variant.BodyStyle?.Name ?? string.Empty,
                        DoorsCount = variant.DoorsCount,
                        YearFrom = variant.YearFrom,
                        YearTo = variant.YearTo,
                        IsDefault = true,
                        PhotoUrl = variant.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? variant.PhotoUrl,
                        Images = variant.Images
                            .OrderByDescending(i => i.IsPrimary)
                            .ThenBy(i => i.SortOrder)
                            .ThenBy(i => i.Id)
                            .Select(i => new GenerationImageDto
                            {
                                Id = i.Id,
                                GenerationVariantId = i.GenerationVariantId,
                                Url = i.Url,
                                IsPrimary = i.IsPrimary,
                                SortOrder = i.SortOrder,
                                CreatedAt = i.CreatedAt
                            })
                            .ToList()
                    }
                },
                Brand = new BrandDto
                {
                    Id = generation.Model!.Brand!.Id,
                    Name = generation.Model.Brand.Name
                },
                Model = new ModelDto
                {
                    Id = generation.Model.Id,
                    Name = generation.Model.Name,
                    BodyType = variant.BodyStyle?.Name ?? string.Empty,
                    BrandId = generation.Model.BrandId
                },
                Trims = variant.Trims
                    .Select(t => new TrimBasicDto
                    {
                        Id = t.Id,
                        GenerationVariantId = variant.Id,
                        Name = t.Name,
                        TransmissionType = t.TransmissionType ?? string.Empty,
                        DoorsCount = t.DoorsCount,
                        SeatsCount = t.SeatsCount,
                        VariantType = variant.VariantType.ToString(),
                        BodyStyleName = variant.BodyStyle?.Name ?? string.Empty
                    })
                    .OrderBy(t => t.Name)
                    .ToList()
            };
        }

        public async Task<TrimFullDto?> GetTrimFullDetailsAsync(int trimId)
        {
            var trim = await _dbContext.Trims
                .Include(t => t.TechnicalDetails)
                .Include(t => t.GenerationVariant)
                    .ThenInclude(v => v!.Images)
                .Include(t => t.GenerationVariant)
                    .ThenInclude(v => v!.BodyStyle)
                .Include(t => t.GenerationVariant)
                    .ThenInclude(v => v!.Generation)
                        .ThenInclude(g => g!.Variants)
                            .ThenInclude(v => v.Images)
                .Include(t => t.GenerationVariant)
                    .ThenInclude(v => v!.Generation)
                        .ThenInclude(g => g!.Model)
                            .ThenInclude(m => m!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == trimId);

            if (trim?.GenerationVariant?.Generation?.Model?.Brand == null)
            {
                return null;
            }

            var variant = trim.GenerationVariant;
            var generation = variant!.Generation!;
            var model = generation.Model!;
            var brand = model.Brand!;

            return new TrimFullDto
            {
                Id = trim.Id,
                Name = trim.Name,
                TransmissionType = trim.TransmissionType ?? string.Empty,
                DoorsCount = trim.DoorsCount,
                SeatsCount = trim.SeatsCount,
                Generation = new GenerationBasicDto
                {
                    Id = generation.Id,
                    Name = generation.Name,
                    YearFrom = generation.YearFrom,
                    YearTo = generation.YearTo,
                    PhotoUrl = generation.Variants
                        .OrderByDescending(v => v.IsDefault)
                        .Select(v => v.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? v.PhotoUrl)
                        .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
                        ?? generation.PhotoUrl
                },
                GenerationVariant = new GenerationVariantBasicDto
                {
                    Id = variant.Id,
                    Name = variant.Name,
                    VariantType = variant.VariantType.ToString(),
                    BodyStyleId = variant.BodyStyleId,
                    BodyStyleName = variant.BodyStyle?.Name ?? string.Empty,
                    DoorsCount = variant.DoorsCount
                },
                Model = new ModelBasicDto
                {
                    Id = model.Id,
                    Name = model.Name
                },
                Brand = new BrandBasicDto
                {
                    Id = brand.Id,
                    Name = brand.Name
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
            int? bodyStyleId = null,
            GenerationVariantType? variantType = null,
            string? transmission = null,
            string? fuelType = null,
            int? brandId = null,
            int? modelId = null,
            int? generationId = null,
            int? generationVariantId = null)
        {
            var query = _dbContext.SearchCars
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (brandId.HasValue)
            {
                query = query.Where(x => x.BrandId == brandId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(x => x.BrandName.Contains(brand));
            }

            if (modelId.HasValue)
            {
                query = query.Where(x => x.ModelId == modelId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(x => x.ModelName.Contains(model));
            }

            // If generationVariantId is provided, use it instead of generationId and variantType
            if (generationVariantId.HasValue)
            {
                query = query.Where(x => x.GenerationVariantId == generationVariantId.Value);
            }
            else
            {
                if (generationId.HasValue)
                {
                    query = query.Where(x => x.GenerationId == generationId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(generation))
                {
                    query = query.Where(x => x.GenerationName.Contains(generation));
                }

                if (variantType.HasValue)
                {
                    query = query.Where(x => x.VariantType == variantType.Value);
                }
            }

            if (minYear.HasValue)
            {
                query = query.Where(x => x.YearTo >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                query = query.Where(x => x.YearFrom <= maxYear.Value);
            }

            if (bodyStyleId.HasValue)
            {
                query = query.Where(x => x.BodyStyleId == bodyStyleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(transmission))
            {
                query = query.Where(x => x.TransmissionType != null && x.TransmissionType == transmission);
            }

            if (!string.IsNullOrWhiteSpace(fuelType))
            {
                query = query.Where(x => x.FuelType != null && x.FuelType == fuelType);
            }

            var rows = await query
                .Select(x => new
                {
                    x.BrandId,
                    x.BrandName,
                    x.ModelId,
                    x.ModelName,
                    x.GenerationId,
                    x.GenerationName,
                    x.GenerationVariantId,
                    x.GenerationVariantName,
                    x.YearFrom,
                    x.YearTo,
                    x.PhotoUrl,
                    x.BodyStyleName
                })
                .ToListAsync();

            var generationCards = rows
                .GroupBy(x => new
                {
                    x.BrandId,
                    x.BrandName,
                    x.ModelId,
                    x.ModelName,
                    x.GenerationId,
                    x.GenerationName,
                    x.GenerationVariantId,
                    x.GenerationVariantName,
                    x.YearFrom,
                    x.YearTo,
                    x.PhotoUrl
                })
                .Select(group => new GenerationCardDto
                {
                    BrandId = group.Key.BrandId,
                    BrandName = group.Key.BrandName,
                    ModelId = group.Key.ModelId,
                    ModelName = group.Key.ModelName,
                    GenerationId = group.Key.GenerationId,
                    GenerationName = group.Key.GenerationName,
                    GenerationVariantId = group.Key.GenerationVariantId,
                    GenerationVariantName = group.Key.GenerationVariantName,
                    DisplayGenerationName = !string.IsNullOrWhiteSpace(group.Key.GenerationVariantName)
                        ? group.Key.GenerationVariantName!
                        : group.Key.GenerationName,
                    BodyType = string.Join(
                        " / ",
                        group.Select(x => x.BodyStyleName)
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct()
                            .Order()),
                    YearFrom = group.Key.YearFrom,
                    YearTo = group.Key.YearTo,
                    PhotoUrl = group.Key.PhotoUrl,
                    TrimCount = group.Count()
                })
                .OrderBy(x => x.BrandName)
                .ThenBy(x => x.ModelName)
                .ThenByDescending(x => x.YearFrom)
                .ToList();

            _logger.LogInformation("Total generation cards found via SearchCars: {Count}", generationCards.Count);
            return generationCards;
        }

        public async Task<SearchFacetsDto> GetSearchFacetsAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            int? brandId = null,
            int? modelId = null,
            int? generationId = null)
        {
            var query = _dbContext.SearchCars
                .AsNoTracking()
                .Where(x => x.IsActive);

            if (brandId.HasValue)
            {
                query = query.Where(x => x.BrandId == brandId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(x => x.BrandName.Contains(brand));
            }

            if (modelId.HasValue)
            {
                query = query.Where(x => x.ModelId == modelId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(x => x.ModelName.Contains(model));
            }

            if (generationId.HasValue)
            {
                query = query.Where(x => x.GenerationId == generationId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(generation))
            {
                query = query.Where(x => x.GenerationName.Contains(generation));
            }

            if (minYear.HasValue)
            {
                query = query.Where(x => x.YearTo >= minYear.Value);
            }

            if (maxYear.HasValue)
            {
                query = query.Where(x => x.YearFrom <= maxYear.Value);
            }

            var bodyStyles = await query
                .Where(x => x.BodyStyleId != null && x.BodyStyleName != null)
                .Select(x => new { x.BodyStyleId, x.BodyStyleName })
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var transmissionTypes = await query
                .Where(x => !string.IsNullOrEmpty(x.TransmissionType))
                .Select(x => x.TransmissionType!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var fuelTypes = await query
                .Where(x => !string.IsNullOrEmpty(x.FuelType))
                .Select(x => x.FuelType!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            return new SearchFacetsDto
            {
                BodyStyles = bodyStyles
                    .ConvertAll(x => new BodyStyleOptionDto
                    {
                        Id = x.BodyStyleId!.Value,
                        Name = x.BodyStyleName!
                    })
,
                TransmissionTypes = transmissionTypes,
                FuelTypes = fuelTypes
            };
        }

        public async Task<IEnumerable<CarModel>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _dbContext.CarModels
                .Where(m => m.BrandId == brandId)
                .Include(m => m.Generations)
                    .ThenInclude(g => g.Variants)
                        .ThenInclude(v => v.Trims)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CarModel?> GetModelByIdAsync(int id)
        {
            return await _dbContext.CarModels
                .Include(m => m.Generations)
                    .ThenInclude(g => g.Variants)
                        .ThenInclude(v => v.Trims)
                            .ThenInclude(t => t.TechnicalDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Generation>> GetGenerationsByModelIdAsync(int modelId)
        {
            return await _dbContext.Generations
                .Where(g => g.ModelId == modelId)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.BodyStyle)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.Images)
                .Include(g => g.Variants)
                        .ThenInclude(v => v.Trims)
                .AsNoTracking()
                .ToListAsync();
        }

            public async Task<IEnumerable<GenerationVariantDto>> GetGenerationVariantsByModelIdAsync(int modelId)
            {
                var variants = await _dbContext.GenerationVariants
                    .Where(variant => variant.Generation != null && variant.Generation.ModelId == modelId)
                    .Include(variant => variant.BodyStyle)
                    .Include(variant => variant.Images)
                    .AsNoTracking()
                    .OrderBy(variant => variant.YearFrom)
                    .ThenBy(variant => variant.BodyStyle!.Name)
                    .ToListAsync();

                return variants.ConvertAll(variant => new GenerationVariantDto
                {
                    Id = variant.Id,
                    GenerationId = variant.GenerationId,
                    Name = variant.Name,
                    VariantType = variant.VariantType.ToString(),
                    BodyStyleId = variant.BodyStyleId,
                    BodyStyleName = variant.BodyStyle?.Name ?? string.Empty,
                    DoorsCount = variant.DoorsCount,
                    YearFrom = variant.YearFrom,
                    YearTo = variant.YearTo,
                    IsDefault = variant.IsDefault,
                    PhotoUrl = variant.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? variant.PhotoUrl,
                    Images = variant.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.SortOrder)
                        .ThenBy(i => i.Id)
                        .Select(i => new GenerationImageDto
                        {
                            Id = i.Id,
                            GenerationVariantId = i.GenerationVariantId,
                            Url = i.Url,
                            IsPrimary = i.IsPrimary,
                            SortOrder = i.SortOrder,
                            CreatedAt = i.CreatedAt
                        })
                        .ToList()
                });
            }

        public async Task<Generation?> GetGenerationByIdAsync(int id)
        {
            return await _dbContext.Generations
                .Include(g => g.Variants)
                    .ThenInclude(v => v.BodyStyle)
                .Include(g => g.Variants)
                    .ThenInclude(v => v.Images)
                .Include(g => g.Variants)
                        .ThenInclude(v => v.Trims)
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Trim>> GetTrimsByGenerationIdAsync(int generationId)
        {
            return await _dbContext.Trims
                .Where(t => t.GenerationVariant != null && t.GenerationVariant.GenerationId == generationId)
                .Include(t => t.TechnicalDetails)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Trim>> GetTrimsByGenerationVariantIdAsync(int generationVariantId)
        {
            return await _dbContext.Trims
                .Where(t => t.GenerationVariantId == generationVariantId)
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
            int? bodyStyleId = null,
            GenerationVariantType? variantType = null,
            string? transmission = null,
            string? fuelType = null)
        {
            var data = await _dbContext.CarBrands
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Images)
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.BodyStyle)
                .Include(b => b.Models)
                    .ThenInclude(m => m.Generations)
                        .ThenInclude(g => g.Variants)
                            .ThenInclude(v => v.Trims)
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

                        var filteredVariants = new List<GenerationVariant>();

                        var variants = genItem.Variants.AsEnumerable();

                        if (variantType.HasValue)
                        {
                            variants = variants.Where(v => v.VariantType == variantType.Value);
                        }

                        if (bodyStyleId.HasValue)
                        {
                            variants = variants.Where(v => v.BodyStyleId == bodyStyleId.Value);
                        }

                        foreach (var variant in variants)
                        {
                            var filteredTrims = variant.Trims.AsEnumerable();

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

                            filteredVariants.Add(new GenerationVariant
                            {
                                Id = variant.Id,
                                GenerationId = genItem.Id,
                                Name = variant.Name,
                                VariantType = variant.VariantType,
                                YearFrom = variant.YearFrom,
                                YearTo = variant.YearTo,
                                IsDefault = variant.IsDefault,
                                PhotoUrl = variant.PhotoUrl,
                                BodyStyleId = variant.BodyStyleId,
                                BodyStyle = variant.BodyStyle,
                                DoorsCount = variant.DoorsCount,
                                Images = variant.Images,
                                Trims = trimsList.ConvertAll(t => new Trim
                                {
                                    Id = t.Id,
                                    Name = t.Name,
                                    GenerationVariantId = variant.Id,
                                    TransmissionType = t.TransmissionType,
                                    DoorsCount = t.DoorsCount,
                                    SeatsCount = t.SeatsCount,
                                    TechnicalDetails = null,
                                    Reviews = new List<Review>()
                                })
                            });
                        }

                        if (filteredVariants.Count == 0)
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
                            PhotoUrl = genItem.Variants
                                .OrderByDescending(v => v.IsDefault)
                                .Select(v => v.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? v.PhotoUrl)
                                .FirstOrDefault(u => !string.IsNullOrWhiteSpace(u))
                                ?? genItem.PhotoUrl,
                            Variants = filteredVariants
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
