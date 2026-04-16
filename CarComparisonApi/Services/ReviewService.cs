using CarComparisonApi.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using CarComparisonApi.Models;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// SQL-backed implementation for managing reviews.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(CarComparisonDbContext dbContext, ILogger<ReviewService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Returns reviews for a specific trim.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of reviews for the trim.</returns>
        public async Task<IEnumerable<Review>> GetReviewsByTrimIdAsync(int trimId)
        {
            return await _dbContext.Reviews
                .Where(r => r.TrimId == trimId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns a review by identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <returns>Review instance or <c>null</c> if not found.</returns>
        public async Task<Review?> GetReviewByIdAsync(int id)
        {
            return await _dbContext.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="review">Review payload to create.</param>
        /// <returns>Created review instance with assigned id and creation date.</returns>
        /// <exception cref="ArgumentException">Thrown when rating is outside 1..10 range.</exception>
        public async Task<Review> CreateReviewAsync(Review review)
        {
            if (review.Rating < 1 || review.Rating > 10)
            {
                _logger.LogWarning("Create review rejected due to invalid rating: {Rating}", review.Rating);
                throw new ArgumentException("Рейтинг має бути в діапазоні від 1 до 10");
            }

            review.CreatedAt = DateTime.UtcNow;
            await _dbContext.Reviews.AddAsync(review);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Review created. ReviewId: {ReviewId}, UserId: {UserId}, TrimId: {TrimId}", review.Id, review.UserId, review.TrimId);
            return review;
        }

        /// <summary>
        /// Updates review content and rating.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <param name="review">Updated review payload.</param>
        /// <exception cref="ArgumentException">Thrown when rating is outside 1..10 range.</exception>
        public async Task UpdateReviewAsync(int id, Review review)
        {
            var existingReview = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (existingReview == null)
            {
                _logger.LogWarning("Attempted to update non-existing review. ReviewId: {ReviewId}", id);
                return;
            }

            if (review.Rating < 1 || review.Rating > 10)
            {
                _logger.LogWarning("Update review rejected due to invalid rating: {Rating}. ReviewId: {ReviewId}", review.Rating, id);
                throw new ArgumentException("Рейтинг має бути в діапазоні від 1 до 10");
            }

            existingReview.Content = review.Content;
            existingReview.Rating = review.Rating;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Review updated. ReviewId: {ReviewId}", id);
        }

        /// <summary>
        /// Deletes review by identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        public async Task DeleteReviewAsync(int id)
        {
            var review = await _dbContext.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
            {
                _logger.LogWarning("Attempted to delete non-existing review. ReviewId: {ReviewId}", id);
                return;
            }

            _dbContext.Reviews.Remove(review);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Review deleted. ReviewId: {ReviewId}", id);
        }

        /// <summary>
        /// Returns all reviews authored by a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Collection of reviews for the user.</returns>
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            return await _dbContext.Reviews
                .Where(r => r.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns reviews for trim enriched with user and car details.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of enriched review projection objects.</returns>
        public async Task<IEnumerable<object>> GetReviewsWithDetailsByTrimIdAsync(int trimId)
        {
            var reviews = await _dbContext.Reviews
                .Where(r => r.TrimId == trimId)
                .Include(r => r.User)
                .Include(r => r.Trim)
                    .ThenInclude(t => t!.Generation)
                        .ThenInclude(g => g!.Model)
                            .ThenInclude(m => m!.Brand)
                .AsNoTracking()
                .ToListAsync();

            return reviews
                .Where(r => r.Trim?.Generation?.Model?.Brand != null)
                .Select(r => new
                {
                    Review = r,
                    Username = r.User?.Username ?? "Невідомий",
                    Model = r.Trim!.Generation!.Model!.Name,
                    Generation = r.Trim.Generation.Name,
                    Trim = r.Trim.Name,
                    Brand = r.Trim.Generation.Model.Brand!.Name
                })
                .ToList();
        }
    }
}
