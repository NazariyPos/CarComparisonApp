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
                    .ThenInclude(t => t!.Generation)
                        .ThenInclude(g => g!.Variants)
                            .ThenInclude(v => v.Images)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.Generation)
                        .ThenInclude(g => g!.Model)
                            .ThenInclude(m => m!.Brand)
                .OrderByDescending(f => f.AddedAt)
                .AsNoTracking()
                .ToListAsync();

            return favorites
                .Where(f => f.Trim?.Generation?.Model?.Brand != null)
                .Select(MapToDto)
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
                    .ThenInclude(t => t!.Generation)
                        .ThenInclude(g => g!.Variants)
                            .ThenInclude(v => v.Images)
                .Include(f => f.Trim)
                    .ThenInclude(t => t!.Generation)
                        .ThenInclude(g => g!.Model)
                            .ThenInclude(m => m!.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return favorite?.Trim?.Generation?.Model?.Brand == null ? null : MapToDto(favorite);
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

        private static FavoriteDto MapToDto(Favorite favorite)
        {
            var trim = favorite.Trim!;
            var generation = trim.Generation!;
            var model = generation.Model!;
            var brand = model.Brand!;

            var photoUrl = generation.Variants
                .OrderByDescending(v => v.IsDefault)
                .Select(v => v.Images.FirstOrDefault(i => i.IsPrimary)?.Url ?? v.PhotoUrl)
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url))
                ?? generation.PhotoUrl;

            return new FavoriteDto
            {
                Id = favorite.Id,
                TrimId = trim.Id,
                TrimName = trim.Name,
                GenerationId = generation.Id,
                GenerationName = generation.Name,
                ModelId = model.Id,
                ModelName = model.Name,
                BrandId = brand.Id,
                BrandName = brand.Name,
                PhotoUrl = photoUrl,
                AddedAt = favorite.AddedAt
            };
        }
    }
}
