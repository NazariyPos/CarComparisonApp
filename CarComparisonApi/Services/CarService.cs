using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CarComparisonApi.Services
{
    public class CarService : ICarService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CarService> _logger;
        private List<CarBrand>? _carData;
        private readonly object _lock = new();
        private readonly string _dataFilePath;

        public CarService(IWebHostEnvironment environment, ILogger<CarService> logger)
        {
            _environment = environment;
            _logger = logger;

            try
            {
                _dataFilePath = Path.Combine(
                    _environment.ContentRootPath,
                    "Data",
                    "cars.json");

                _logger.LogInformation("Шлях до даних: {DataFilePath}", _dataFilePath);
                LoadData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при ініціалізації CarService");
                throw;
            }
        }

        private void LoadData()
        {
            lock (_lock)
            {
                try
                {
                    _logger.LogInformation("Завантаження даних з {DataFilePath}", _dataFilePath);

                    var dataDirectory = Path.GetDirectoryName(_dataFilePath);
                    if (string.IsNullOrEmpty(dataDirectory))
                    {
                        _logger.LogWarning("Не вдалося визначити директорію для шляху {DataFilePath}", _dataFilePath);
                        return;
                    }

                    if (!Directory.Exists(dataDirectory))
                    {
                        Directory.CreateDirectory(dataDirectory);
                        _logger.LogInformation("Створено папку {DataDirectory}", dataDirectory);
                    }

                    if (File.Exists(_dataFilePath))
                    {
                        var json = File.ReadAllText(_dataFilePath);
                        _carData = JsonConvert.DeserializeObject<List<CarBrand>>(json);
                        _logger.LogInformation("Завантажено {BrandCount} марок авто", _carData?.Count ?? 0);
                    }
                    else
                    {
                        _logger.LogWarning("Файл {DataFilePath} не знайдено.", _dataFilePath);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Помилка при парсингу JSON");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка при завантаженні даних");
                    _carData = new List<CarBrand>();
                }
            }
        }

        public Task<IEnumerable<CarBrand>> GetAllBrandsAsync()
        {
            return Task.FromResult(_carData?.AsEnumerable() ?? Enumerable.Empty<CarBrand>());
        }

        public Task<CarBrand?> GetBrandByIdAsync(int id)
        {
            var brand = _carData?.FirstOrDefault(b => b.Id == id);
            return Task.FromResult(brand);
        }

        public Task<GenerationWithTrimsDto?> GetGenerationWithTrimsAsync(int generationId)
        {
            foreach (var brand in _carData ?? new List<CarBrand>())
            {
                foreach (var model in brand.Models)
                {
                    var generation = model.Generations.FirstOrDefault(g => g.Id == generationId);
                    if (generation != null)
                    {
                        var result = new GenerationWithTrimsDto
                        {
                            Id = generation.Id,
                            Name = generation.Name,
                            YearFrom = generation.YearFrom,
                            YearTo = generation.YearTo,
                            PhotoUrl = generation.PhotoUrl,
                            Brand = new BrandDto
                            {
                                Id = brand.Id,
                                Name = brand.Name
                            },
                            Model = new ModelDto
                            {
                                Id = model.Id,
                                Name = model.Name,
                                BodyType = model.BodyType ?? string.Empty,
                                BrandId = model.BrandId
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

                        return Task.FromResult<GenerationWithTrimsDto?>(result);
                    }
                }
            }

            return Task.FromResult<GenerationWithTrimsDto?>(null);
        }
        public Task<TrimFullDto?> GetTrimFullDetailsAsync(int trimId)
        {
            foreach (var brand in _carData ?? new List<CarBrand>())
            {
                foreach (var model in brand.Models)
                {
                    foreach (var generation in model.Generations)
                    {
                        var trim = generation.Trims.FirstOrDefault(t => t.Id == trimId);
                        if (trim != null)
                        {
                            var result = new TrimFullDto
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
                                    PhotoUrl = generation.PhotoUrl
                                },
                                Model = new ModelBasicDto
                                {
                                    Id = model.Id,
                                    Name = model.Name,
                                    BodyType = model.BodyType ?? string.Empty
                                },
                                Brand = new BrandBasicDto
                                {
                                    Id = brand.Id,
                                    Name = brand.Name
                                },
                                TechnicalDetails = trim.TechnicalDetails
                            };

                            return Task.FromResult<TrimFullDto?>(result);
                        }
                    }
                }
            }

            return Task.FromResult<TrimFullDto?>(null);
        }

        public Task<IEnumerable<GenerationCardDto>> GetGenerationCardsAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null)
        {
            var filteredBrands = _carData?.AsEnumerable() ?? Enumerable.Empty<CarBrand>();

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

                        if (minYear.HasValue)
                        {
                            if (genItem.YearFrom < minYear.Value) continue;
                        }

                        if (maxYear.HasValue)
                        {
                            if (genItem.YearFrom > maxYear.Value) continue;
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
                            continue;

                        var card = new GenerationCardDto
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
                            TrimCount = trimsList.Count,
                        };

                        generationCards.Add(card);
                    }
                }
            }

            Console.WriteLine($"Total generation cards found: {generationCards.Count}");
            Console.WriteLine("=== End GetGenerationCardsAsync ===");

            return Task.FromResult(generationCards.AsEnumerable());
        }

        public Task<IEnumerable<CarModel>> GetModelsByBrandIdAsync(int brandId)
        {
            var models = _carData?
                .Where(b => b.Id == brandId)
                .SelectMany(b => b.Models)
                .ToList() ?? new List<CarModel>();
            return Task.FromResult(models.AsEnumerable());
        }

        public Task<CarModel?> GetModelByIdAsync(int id)
        {
            var model = _carData?
                .SelectMany(b => b.Models)
                .FirstOrDefault(m => m.Id == id);
            return Task.FromResult(model);
        }

        public Task<IEnumerable<Generation>> GetGenerationsByModelIdAsync(int modelId)
        {
            var generations = _carData?
                .SelectMany(b => b.Models)
                .Where(m => m.Id == modelId)
                .SelectMany(m => m.Generations)
                .ToList() ?? new List<Generation>();
            return Task.FromResult(generations.AsEnumerable());
        }

        public Task<Generation?> GetGenerationByIdAsync(int id)
        {
            var generation = _carData?
                .SelectMany(b => b.Models)
                .SelectMany(m => m.Generations)
                .FirstOrDefault(g => g.Id == id);
            return Task.FromResult(generation);
        }

        public Task<IEnumerable<Trim>> GetTrimsByGenerationIdAsync(int generationId)
        {
            var trims = _carData?
                .SelectMany(b => b.Models)
                .SelectMany(m => m.Generations)
                .Where(g => g.Id == generationId)
                .SelectMany(g => g.Trims)
                .ToList() ?? new List<Trim>();
            return Task.FromResult(trims.AsEnumerable());
        }

        public Task<Trim?> GetTrimByIdAsync(int id)
        {
            var trim = _carData?
                .SelectMany(b => b.Models)
                .SelectMany(m => m.Generations)
                .SelectMany(g => g.Trims)
                .FirstOrDefault(t => t.Id == id);
            return Task.FromResult(trim);
        }

        public Task<TechnicalDetails?> GetTechnicalDetailsByTrimIdAsync(int trimId)
        {
            var trim = _carData?
                .SelectMany(b => b.Models)
                .SelectMany(m => m.Generations)
                .SelectMany(g => g.Trims)
                .FirstOrDefault(t => t.Id == trimId);
            return Task.FromResult(trim?.TechnicalDetails);
        }

        public Task<IEnumerable<CarBrand>> SearchAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null)
        {
            var result = _carData?.AsEnumerable() ?? Enumerable.Empty<CarBrand>();

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
                            continue;
                        if (maxYear.HasValue && genItem.YearFrom > maxYear.Value)
                            continue;

                        var filteredTrims = genItem.Trims.AsEnumerable();

                        if (!string.IsNullOrEmpty(transmission))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TransmissionType?.Contains(transmission, StringComparison.OrdinalIgnoreCase) == true);
                        }

                        if (!string.IsNullOrEmpty(fuelType))
                        {
                            filteredTrims = filteredTrims.Where(t =>
                                t.TechnicalDetails != null &&
                                t.TechnicalDetails.FuelType?.Contains(fuelType, StringComparison.OrdinalIgnoreCase) == true);
                        }

                        var trimsList = filteredTrims.ToList();
                        if (trimsList.Count == 0)
                            continue;

                        var simplifiedGeneration = new Generation
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
                        };

                        filteredGenerations.Add(simplifiedGeneration);
                    }

                    if (filteredGenerations.Count == 0)
                        continue;

                    var simplifiedModel = new CarModel
                    {
                        Id = modelItem.Id,
                        Name = modelItem.Name,
                        BrandId = modelItem.BrandId,
                        BodyType = modelItem.BodyType,
                        Generations = filteredGenerations
                    };

                    filteredModels.Add(simplifiedModel);
                }

                if (filteredModels.Count == 0)
                    continue;

                var simplifiedBrand = new CarBrand
                {
                    Id = brandItem.Id,
                    Name = brandItem.Name,
                    Models = filteredModels
                };

                simplifiedBrands.Add(simplifiedBrand);
            }

            return Task.FromResult(simplifiedBrands.AsEnumerable());
        }

        public Task<IEnumerable<Trim>> GetTrimsForComparisonAsync(IEnumerable<int> trimIds)
        {
            var trims = _carData?
                .SelectMany(b => b.Models)
                .SelectMany(m => m.Generations)
                .SelectMany(g => g.Trims)
                .Where(t => trimIds.Contains(t.Id))
                .Take(4)
                .ToList() ?? new List<Trim>();

            return Task.FromResult(trims.AsEnumerable());
        }
    }
}
