using CarComparisonApi.Models;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides CRUD operations for trim reviews.
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Returns reviews for a specific trim.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of reviews.</returns>
        Task<IEnumerable<Review>> GetReviewsByTrimIdAsync(int trimId);

        /// <summary>
        /// Returns a review by identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <returns>Review instance or <c>null</c> if not found.</returns>
        Task<Review?> GetReviewByIdAsync(int id);

        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="review">Review payload to create.</param>
        /// <returns>Created review instance.</returns>
        Task<Review> CreateReviewAsync(Review review);

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <param name="review">Updated review payload.</param>
        Task UpdateReviewAsync(int id, Review review);

        /// <summary>
        /// Deletes a review.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        Task DeleteReviewAsync(int id);

        /// <summary>
        /// Returns reviews authored by a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Collection of user reviews.</returns>
        Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId);

        /// <summary>
        /// Returns reviews enriched with user and car hierarchy details.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of enriched review projections.</returns>
        Task<IEnumerable<object>> GetReviewsWithDetailsByTrimIdAsync(int trimId);
    }
}
