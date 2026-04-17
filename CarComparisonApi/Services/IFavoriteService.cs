using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides operations for managing user favorites.
    /// </summary>
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(int userId);
        Task<FavoriteDto?> AddFavoriteAsync(int userId, int trimId);
        Task<bool> RemoveFavoriteAsync(int userId, int trimId);
        Task<bool> IsFavoriteAsync(int userId, int trimId);
    }
}
