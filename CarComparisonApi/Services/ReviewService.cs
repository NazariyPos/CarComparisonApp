using CarComparisonApi.Models;
using Newtonsoft.Json;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// JSON-backed implementation for managing reviews.
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly ICarService _carService;
        private readonly IAuthService _authService;
        private readonly string _reviewsFilePath;
        private List<Review> _reviews = new();
        private readonly object _lock = new();

        /// <summary>
        /// Initializes review service and loads persisted reviews.
        /// </summary>
        /// <param name="environment">Host environment used to resolve review file path.</param>
        /// <param name="carService">Car catalog service for enrichment of review responses.</param>
        /// <param name="authService">Authentication service used to resolve user information.</param>
        public ReviewService(IWebHostEnvironment environment, ICarService carService, IAuthService authService)
        {
            _carService = carService;
            _authService = authService;
            _reviewsFilePath = Path.Combine(environment.ContentRootPath, "Data", "reviews.json");
            LoadReviews();
        }

        private void LoadReviews()
        {
            lock (_lock)
            {
                if (File.Exists(_reviewsFilePath))
                {
                    var json = File.ReadAllText(_reviewsFilePath);
                    _reviews = JsonConvert.DeserializeObject<List<Review>>(json) ?? new List<Review>();
                }
                else
                {
                    _reviews = new List<Review>
                    {
                        new Review
                        {
                            Id = 1,
                            UserId = 1,
                            TrimId = 1,
                            Content = "Чудовий автомобіль, дуже задоволений покупкою! Комфорт на високому рівні.",
                            Rating = 9,
                            CreatedAt = DateTime.UtcNow.AddDays(-30)
                        },
                        new Review
                        {
                            Id = 2,
                            UserId = 1,
                            TrimId = 2,
                            Content = "Потужний двигун, але велика витрата палива в місті.",
                            Rating = 7,
                            CreatedAt = DateTime.UtcNow.AddDays(-15)
                        },
                        new Review
                        {
                            Id = 3,
                            UserId = 1,
                            TrimId = 3,
                            Content = "Надійний кросовер, ідеально підходить для сім'ї.",
                            Rating = 8,
                            CreatedAt = DateTime.UtcNow.AddDays(-10)
                        }
                    };
                    SaveReviews();
                }
            }
        }

        private void SaveReviews()
        {
            var json = JsonConvert.SerializeObject(_reviews, Formatting.Indented);
            File.WriteAllText(_reviewsFilePath, json);
        }

        /// <summary>
        /// Returns reviews for a specific trim.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of reviews for the trim.</returns>
        public Task<IEnumerable<Review>> GetReviewsByTrimIdAsync(int trimId)
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<Review>>(_reviews.Where(r => r.TrimId == trimId).ToList());
            }
        }

        /// <summary>
        /// Returns a review by identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <returns>Review instance or <c>null</c> if not found.</returns>
        public Task<Review?> GetReviewByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_reviews.FirstOrDefault(r => r.Id == id));
            }
        }

        /// <summary>
        /// Creates a new review.
        /// </summary>
        /// <param name="review">Review payload to create.</param>
        /// <returns>Created review instance with assigned id and creation date.</returns>
        /// <exception cref="ArgumentException">Thrown when rating is outside 1..10 range.</exception>
        public Task<Review> CreateReviewAsync(Review review)
        {
            lock (_lock)
            {
                if (review.Rating < 1 || review.Rating > 10)
                    throw new ArgumentException("Рейтинг має бути в діапазоні від 1 до 10");

                review.Id = _reviews.Count > 0 ? _reviews.Max(r => r.Id) + 1 : 1;
                review.CreatedAt = DateTime.UtcNow;
                _reviews.Add(review);
                SaveReviews();
                return Task.FromResult(review);
            }
        }

        /// <summary>
        /// Updates review content and rating.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        /// <param name="review">Updated review payload.</param>
        /// <exception cref="ArgumentException">Thrown when rating is outside 1..10 range.</exception>
        public Task UpdateReviewAsync(int id, Review review)
        {
            lock (_lock)
            {
                var existingReview = _reviews.FirstOrDefault(r => r.Id == id);
                if (existingReview != null)
                {
                    if (review.Rating < 1 || review.Rating > 10)
                        throw new ArgumentException("Рейтинг має бути в діапазоні від 1 до 10");

                    existingReview.Content = review.Content;
                    existingReview.Rating = review.Rating;
                    existingReview.UpdatedAt = DateTime.UtcNow;
                    SaveReviews();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Deletes review by identifier.
        /// </summary>
        /// <param name="id">Review identifier.</param>
        public Task DeleteReviewAsync(int id)
        {
            lock (_lock)
            {
                var review = _reviews.FirstOrDefault(r => r.Id == id);
                if (review != null)
                {
                    _reviews.Remove(review);
                    SaveReviews();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns all reviews authored by a specific user.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Collection of reviews for the user.</returns>
        public Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userId)
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<Review>>(_reviews.Where(r => r.UserId == userId).ToList());
            }
        }

        /// <summary>
        /// Returns reviews for trim enriched with user and car details.
        /// </summary>
        /// <param name="trimId">Trim identifier.</param>
        /// <returns>Collection of enriched review projection objects.</returns>
        public async Task<IEnumerable<object>> GetReviewsWithDetailsByTrimIdAsync(int trimId)
        {
            var reviews = await GetReviewsByTrimIdAsync(trimId);
            var result = new List<object>();

            foreach (var review in reviews)
            {
                var user = await _authService.GetUserByIdAsync(review.UserId);
                var trim = await _carService.GetTrimByIdAsync(review.TrimId);

                if (trim != null)
                {
                    var generation = await _carService.GetGenerationByIdAsync(trim.GenerationId);
                    if (generation != null)
                    {
                        var model = await _carService.GetModelByIdAsync(generation.ModelId);
                        if (model != null)
                        {
                            var brand = await _carService.GetBrandByIdAsync(model.BrandId);
                            if (brand != null)
                            {
                                result.Add(new
                                {
                                    Review = review,
                                    Username = user?.Username ?? "Невідомий",
                                    Model = model.Name,
                                    Generation = generation.Name,
                                    Trim = trim.Name,
                                    Brand = brand.Name
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
