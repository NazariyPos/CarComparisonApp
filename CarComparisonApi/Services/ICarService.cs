using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides read/query operations for brands, models, generations and trims.
    /// </summary>
    public interface ICarService
    {
        /// <summary>
        /// Returns all available car brands.
        /// </summary>
        /// <returns>Collection of brands.</returns>
        Task<IEnumerable<CarBrand>> GetAllBrandsAsync();

        /// <summary>
        /// Returns a brand by identifier.
        /// </summary>
        /// <param name="id">Brand identifier.</param>
        /// <returns>Brand instance or <c>null</c> if not found.</returns>
        Task<CarBrand?> GetBrandByIdAsync(int id);

        /// <summary>
        /// Returns all models that belong to the specified brand.
        /// </summary>
        /// <param name="brandId">Brand identifier.</param>
        /// <returns>Collection of models for the brand.</returns>
        Task<IEnumerable<CarModel>> GetModelsByBrandIdAsync(int brandId);

        /// <summary>
        /// Returns a model by identifier.
        /// </summary>
        /// <param name="id">Model identifier.</param>
        /// <returns>Model instance or <c>null</c> if not found.</returns>
        Task<CarModel?> GetModelByIdAsync(int id);

        /// <summary>
        /// Returns all generations for the specified model.
        /// </summary>
        /// <param name="modelId">Model identifier.</param>
        /// <returns>Collection of generations.</returns>
        Task<IEnumerable<Generation>> GetGenerationsByModelIdAsync(int modelId);

        /// <summary>
        /// Returns a generation by identifier.
        /// </summary>
        /// <param name="id">Generation identifier.</param>
        /// <returns>Generation instance or <c>null</c> if not found.</returns>
        Task<Generation?> GetGenerationByIdAsync(int id);

        /// <summary>
        /// Returns all trims for the specified generation.
        /// </summary>
        /// <param name="generationId">Generation identifier.</param>
        /// <returns>Collection of trims.</returns>
        Task<IEnumerable<Trim>> GetTrimsByGenerationIdAsync(int generationId);

        /// <summary>
        /// Returns a trim by identifier.
        /// </summary>
        /// <param name="id">Trim identifier.</param>
        /// <returns>Trim instance or <c>null</c> if not found.</returns>
        Task<Trim?> GetTrimByIdAsync(int id);

        /// <summary>
        /// Returns technical details for the specified trim.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Technical details or <c>null</c> if not found.</returns>
        Task<TechnicalDetails?> GetTechnicalDetailsByTrimIdAsync(int trimId);

        /// <summary>
        /// Searches the catalog and returns a filtered car hierarchy.
        /// </summary>
        /// <param name="brand">Optional brand filter.</param>
        /// <param name="model">Optional model filter.</param>
        /// <param name="generation">Optional generation filter.</param>
        /// <param name="minYear">Optional minimum year filter.</param>
        /// <param name="maxYear">Optional maximum year filter.</param>
        /// <param name="bodyType">Optional body type filter.</param>
        /// <param name="transmission">Optional transmission filter.</param>
        /// <param name="fuelType">Optional fuel type filter.</param>
        /// <returns>Filtered hierarchy of brands/models/generations/trims.</returns>
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
        /// <param name="brand">Optional brand filter.</param>
        /// <param name="model">Optional model filter.</param>
        /// <param name="generation">Optional generation filter.</param>
        /// <param name="minYear">Optional minimum year filter.</param>
        /// <param name="maxYear">Optional maximum year filter.</param>
        /// <param name="bodyType">Optional body type filter.</param>
        /// <param name="transmission">Optional transmission filter.</param>
        /// <param name="fuelType">Optional fuel type filter.</param>
        /// <returns>Collection of generation card DTOs.</returns>
        Task<IEnumerable<GenerationCardDto>> GetGenerationCardsAsync(
            string? brand = null,
            string? model = null,
            string? generation = null,
            int? minYear = null,
            int? maxYear = null,
            string? bodyType = null,
            string? transmission = null,
            string? fuelType = null);

        /// <summary>
        /// Returns generation details including trims.
        /// </summary>
        /// <param name="generationId">Generation identifier.</param>
        /// <returns>Detailed generation DTO or <c>null</c> if not found.</returns>
        Task<GenerationWithTrimsDto?> GetGenerationWithTrimsAsync(int generationId);

        /// <summary>
        /// Returns full trim details including hierarchy and technical specs.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Detailed trim DTO or <c>null</c> if not found.</returns>
        Task<TrimFullDto?> GetTrimFullDetailsAsync(int trimId);

        /// <summary>
        /// Returns trims selected for comparison.
        /// </summary>
        /// <param name="trimIds">Identifiers of trims to compare.</param>
        /// <returns>Collection of trims prepared for comparison.</returns>
        Task<IEnumerable<Trim>> GetTrimsForComparisonAsync(IEnumerable<int> trimIds);
    }
}
