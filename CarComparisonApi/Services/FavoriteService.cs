using CarComparisonApi.Data;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// SQL-backed service for user favorite trims.
    /// </summary>
    public class FavoriteService : IFavoriteService
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(CarComparisonDbContext dbContext, ILogger<FavoriteService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(int userId)
        {
            var favorites = await _dbContext.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.GenerationVariant)
                        .ThenInclude(v => v!.Images)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.GenerationVariant)
                        .ThenInclude(v => v!.Model)
                            .ThenInclude(m => m!.Brand)
                .OrderByDescending(f => f.AddedAt)
                .AsNoTracking()
                .ToListAsync();

            var generationVariantIds = favorites
                .Select(f => f.Trim?.GenerationVariant?.Id)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var trimCountsByVariantId = await _dbContext.Trims
                .Where(t => generationVariantIds.Contains(t.GenerationVariantId))
                .GroupBy(t => t.GenerationVariantId)
                .Select(g => new { GenerationVariantId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.GenerationVariantId, x => x.Count);

            return favorites
                .Where(f => f.Trim?.GenerationVariant?.Model?.Brand != null)
                .Select(f => MapToDto(f, GetTrimCount(f, trimCountsByVariantId)))
                .ToList();
        }

        public async Task<FavoriteDto?> AddFavoriteAsync(int userId, int trimId)
        {
            var trimExists = await _dbContext.Trims.AnyAsync(t => t.Id == trimId);
            if (!trimExists)
            {
                return null;
            }

            var existing = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TrimId == trimId);

            if (existing == null)
            {
                existing = new Favorite
                {
                    UserId = userId,
                    TrimId = trimId,
                    AddedAt = DateTime.UtcNow
                };

                await _dbContext.Favorites.AddAsync(existing);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Favorite added. UserId: {UserId}, TrimId: {TrimId}", userId, trimId);
            }

            var favorite = await _dbContext.Favorites
                .Where(f => f.Id == existing.Id)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.GenerationVariant)
                        .ThenInclude(v => v!.Images)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.GenerationVariant)
                        .ThenInclude(v => v!.Model)
                            .ThenInclude(m => m!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return favorite?.Trim?.GenerationVariant?.Model?.Brand == null
                ? null
                : MapToDto(favorite, await GetTrimCountAsync(favorite));
        }

        public async Task<bool> RemoveFavoriteAsync(int userId, int trimId)
        {
            var favorite = await _dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.TrimId == trimId);

            if (favorite == null)
            {
                return false;
            }

            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Favorite removed. UserId: {UserId}, TrimId: {TrimId}", userId, trimId);
            return true;
        }

        public Task<bool> IsFavoriteAsync(int userId, int trimId)
        {
            return _dbContext.Favorites.AnyAsync(f => f.UserId == userId && f.TrimId == trimId);
        }

        private async Task<int> GetTrimCountAsync(Favorite favorite)
        {
            var generationVariantId = favorite.Trim?.GenerationVariant?.Id;
            if (!generationVariantId.HasValue)
            {
                return 0;
            }

            return await _dbContext.Trims.CountAsync(t => t.GenerationVariantId == generationVariantId.Value);
        }

        private static int GetTrimCount(
            Favorite favorite,
            Dictionary<int, int> trimCountsByVariantId)
        {
            var generationVariantId = favorite.Trim?.GenerationVariant?.Id;
            if (!generationVariantId.HasValue)
            {
                return 0;
            }

            return trimCountsByVariantId.TryGetValue(generationVariantId.Value, out var count)
                ? count
                : 0;
        }

        private static FavoriteDto MapToDto(Favorite favorite, int trimCount)
        {
            var trim = favorite.Trim!;
            var variant = trim.GenerationVariant!;
            var model = variant.Model!;
            var brand = model.Brand!;

            var photoUrl = variant.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                ?? variant.PhotoUrl;

            return new FavoriteDto
            {
                Id = favorite.Id,
                TrimId = trim.Id,
                TrimName = trim.Name,
                GenerationId = variant.Id,
                GenerationName = variant.Name,
                DisplayGenerationName = variant.Name,
                YearFrom = variant.YearFrom,
                YearTo = variant.YearTo,
                ModelId = model.Id,
                ModelName = model.Name,
                BrandId = brand.Id,
                BrandName = brand.Name,
                TrimCount = trimCount,
                PhotoUrl = photoUrl,
                AddedAt = favorite.AddedAt
            };
        }
    }
}
