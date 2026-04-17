using CarComparisonApi.Data;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Handles upload and metadata operations for generation variant images.
    /// </summary>
    public class GenerationImageService : IGenerationImageService
    {
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private readonly CarComparisonDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<GenerationImageService> _logger;

        public GenerationImageService(
            CarComparisonDbContext dbContext,
            IWebHostEnvironment environment,
            ILogger<GenerationImageService> logger)
        {
            _dbContext = dbContext;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IEnumerable<GenerationImageDto>> GetByVariantIdAsync(int generationId, int variantId)
        {
            var images = await _dbContext.GenerationImages
                .Where(i => i.GenerationVariantId == variantId && i.GenerationVariant!.GenerationId == generationId)
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .AsNoTracking()
                .ToListAsync();

            return images.Select(ToDto);
        }

        public async Task<GenerationImageDto?> UploadAsync(int generationId, int variantId, IFormFile file, bool isPrimary, int? sortOrder)
        {
            var variant = await _dbContext.GenerationVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.GenerationId == generationId);

            if (variant == null)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Дозволені формати: .jpg, .jpeg, .png, .webp");
            }

            if (file.Length <= 0)
            {
                throw new InvalidOperationException("Файл порожній.");
            }

            const long maxSizeBytes = 10 * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                throw new InvalidOperationException("Максимальний розмір файлу: 10 MB.");
            }

            var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;

            var relativeDirectory = Path.Combine("uploads", "generations", generationId.ToString(), "variants", variantId.ToString());
            var physicalDirectory = Path.Combine(webRootPath, relativeDirectory);
            Directory.CreateDirectory(physicalDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var physicalPath = Path.Combine(physicalDirectory, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (isPrimary)
            {
                var previousPrimary = await _dbContext.GenerationImages
                    .Where(i => i.GenerationVariantId == variantId && i.IsPrimary)
                    .ToListAsync();

                foreach (var image in previousPrimary)
                {
                    image.IsPrimary = false;
                }
            }

            var computedSortOrder = sortOrder ?? await GetNextSortOrderAsync(variantId);
            var url = $"/uploads/generations/{generationId}/variants/{variantId}/{fileName}";

            var entity = new GenerationImage
            {
                GenerationVariantId = variantId,
                Url = url,
                FileName = fileName,
                IsPrimary = isPrimary,
                SortOrder = computedSortOrder,
                CreatedAt = DateTime.UtcNow
            };

            await _dbContext.GenerationImages.AddAsync(entity);

            if (isPrimary || string.IsNullOrWhiteSpace(variant.PhotoUrl))
            {
                variant.PhotoUrl = entity.Url;
            }

            if (variant.IsDefault)
            {
                var generation = await _dbContext.Generations.FirstAsync(g => g.Id == generationId);
                if (isPrimary || string.IsNullOrWhiteSpace(generation.PhotoUrl))
                {
                    generation.PhotoUrl = entity.Url;
                }
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Image uploaded for generation {GenerationId}, variant {VariantId}. ImageId: {ImageId}", generationId, variantId, entity.Id);
            return ToDto(entity);
        }

        public async Task<bool> DeleteAsync(int generationId, int variantId, int imageId)
        {
            var image = await _dbContext.GenerationImages
                .Include(i => i.GenerationVariant)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.GenerationVariantId == variantId && i.GenerationVariant!.GenerationId == generationId);

            if (image == null)
            {
                return false;
            }

            var webRootPath = string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(_environment.ContentRootPath, "wwwroot")
                : _environment.WebRootPath;
            var fullPath = Path.Combine(webRootPath, image.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            _dbContext.GenerationImages.Remove(image);
            await _dbContext.SaveChangesAsync();

            if (image.IsPrimary)
            {
                var fallback = await _dbContext.GenerationImages
                    .Where(i => i.GenerationVariantId == variantId)
                    .OrderBy(i => i.SortOrder)
                    .ThenBy(i => i.Id)
                    .FirstOrDefaultAsync();

                var variant = await _dbContext.GenerationVariants.FirstOrDefaultAsync(v => v.Id == variantId && v.GenerationId == generationId);
                if (variant != null)
                {
                    variant.PhotoUrl = fallback?.Url;
                    if (variant.IsDefault)
                    {
                        var generation = await _dbContext.Generations.FirstOrDefaultAsync(g => g.Id == generationId);
                        if (generation != null)
                        {
                            generation.PhotoUrl = fallback?.Url;
                        }
                    }
                }

                if (fallback != null)
                {
                    fallback.IsPrimary = true;
                }

                await _dbContext.SaveChangesAsync();
            }

            _logger.LogInformation("Image deleted. GenerationId: {GenerationId}, VariantId: {VariantId}, ImageId: {ImageId}", generationId, variantId, imageId);
            return true;
        }

        public async Task<GenerationImageDto?> SetPrimaryAsync(int generationId, int variantId, int imageId)
        {
            var variant = await _dbContext.GenerationVariants
                .FirstOrDefaultAsync(v => v.Id == variantId && v.GenerationId == generationId);
            if (variant == null)
            {
                return null;
            }

            var images = await _dbContext.GenerationImages
                .Where(i => i.GenerationVariantId == variantId)
                .ToListAsync();

            if (images.Count == 0)
            {
                return null;
            }

            var target = images.FirstOrDefault(i => i.Id == imageId);
            if (target == null)
            {
                return null;
            }

            foreach (var image in images)
            {
                image.IsPrimary = image.Id == imageId;
            }

            variant.PhotoUrl = target.Url;
            if (variant.IsDefault)
            {
                var generation = await _dbContext.Generations.FirstOrDefaultAsync(g => g.Id == generationId);
                if (generation != null)
                {
                    generation.PhotoUrl = target.Url;
                }
            }

            await _dbContext.SaveChangesAsync();
            return ToDto(target);
        }

        private async Task<int> GetNextSortOrderAsync(int variantId)
        {
            var maxSortOrder = await _dbContext.GenerationImages
                .Where(i => i.GenerationVariantId == variantId)
                .Select(i => (int?)i.SortOrder)
                .MaxAsync();

            return (maxSortOrder ?? 0) + 1;
        }

        private static GenerationImageDto ToDto(GenerationImage image)
        {
            return new GenerationImageDto
            {
                Id = image.Id,
                GenerationVariantId = image.GenerationVariantId,
                Url = image.Url,
                IsPrimary = image.IsPrimary,
                SortOrder = image.SortOrder,
                CreatedAt = image.CreatedAt
            };
        }
    }
}
