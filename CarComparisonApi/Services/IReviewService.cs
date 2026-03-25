using CarComparisonApi.Models;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides CRUD operations for trim reviews.
    /// </summary>
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByTrimIdAsync(int trimId);
        Task<Review?> GetReviewByIdAsync(int id);
        Task<Review> CreateReviewAsync(Review review);
        Task UpdateReviewAsync(int id, Review review);
        Task DeleteReviewAsync(int id);
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);

        /// <summary>
        /// Returns reviews enriched with user and car hierarchy details.
        /// </summary>
        Task<IEnumerable<object>> GetReviewsWithDetailsByTrimIdAsync(int trimId);
    }
}
