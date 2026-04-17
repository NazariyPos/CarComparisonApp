using CarComparisonApi.Models.DTOs;
using Microsoft.AspNetCore.Http;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides operations for generation images storage and metadata.
    /// </summary>
    public interface IGenerationImageService
    {
        Task<IEnumerable<GenerationImageDto>> GetByVariantIdAsync(int generationId, int variantId);
        Task<GenerationImageDto?> UploadAsync(int generationId, int variantId, IFormFile file, bool isPrimary, int? sortOrder);
        Task<bool> DeleteAsync(int generationId, int variantId, int imageId);
        Task<GenerationImageDto?> SetPrimaryAsync(int generationId, int variantId, int imageId);
    }
}
