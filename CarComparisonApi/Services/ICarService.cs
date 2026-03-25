using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides read/query operations for brands, models, generations and trims.
    /// </summary>
    public interface ICarService
    {
        Task<IEnumerable<CarBrand>> GetAllBrandsAsync();
        Task<CarBrand?> GetBrandByIdAsync(int id);
        Task<IEnumerable<CarModel>> GetModelsByBrandIdAsync(int brandId);
        Task<CarModel?> GetModelByIdAsync(int id);
        Task<IEnumerable<Generation>> GetGenerationsByModelIdAsync(int modelId);
        Task<Generation?> GetGenerationByIdAsync(int id);
        Task<IEnumerable<Trim>> GetTrimsByGenerationIdAsync(int generationId);
        Task<Trim?> GetTrimByIdAsync(int id);
        Task<TechnicalDetails?> GetTechnicalDetailsByTrimIdAsync(int trimId);

        /// <summary>
        /// Searches the catalog and returns a filtered car hierarchy.
        /// </summary>
        Task<IEnumerable<CarBrand>> SearchAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null);

        /// <summary>
        /// Returns generation cards for search UI.
        /// </summary>
        Task<IEnumerable<GenerationCardDto>> GetGenerationCardsAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null);

        Task<GenerationWithTrimsDto?> GetGenerationWithTrimsAsync(int generationId);
        Task<TrimFullDto?> GetTrimFullDetailsAsync(int trimId);
        Task<IEnumerable<Trim>> GetTrimsForComparisonAsync(IEnumerable<int> trimIds);
    }
}
