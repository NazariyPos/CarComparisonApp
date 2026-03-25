using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarComparisonApi.Models;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Provides endpoints for creating, reading, updating and deleting reviews.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly ICarService _carService;
        private readonly IAuthService _authService;

        public ReviewsController(IReviewService reviewService, ICarService carService, IAuthService authService)
        {
            _reviewService = reviewService;
            _carService = carService;
            _authService = authService;
        }

        /// <summary>
        /// Returns reviews for a specific trim.
        /// </summary>
        [HttpGet("trim/{trimId}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get reviews by trim")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviewsByTrim(int trimId)
        {
            var reviewsWithDetails = await _reviewService.GetReviewsWithDetailsByTrimIdAsync(trimId);
            return Ok(reviewsWithDetails);
        }

        /// <summary>
        /// Creates a new review for a trim.
        /// </summary>
        [HttpPost]
        [Authorize]
        [SwaggerOperation(Summary = "Create review")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateReview([FromBody] Review review)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            if (review.Rating < 1 || review.Rating > 10)
                return BadRequest("Рейтинг має бути в діапазоні від 1 до 10");

            review.UserId = userId.Value;
            review.CreatedAt = DateTime.UtcNow;

            var createdReview = await _reviewService.CreateReviewAsync(review);

            var reviewsWithDetails = await _reviewService.GetReviewsWithDetailsByTrimIdAsync(review.TrimId);
            var newReview = reviewsWithDetails.FirstOrDefault(r =>
                ((dynamic)r).Review.Id == createdReview.Id);

            return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, newReview);
        }

        /// <summary>
        /// Returns a single review by identifier.
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get review by ID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            var reviewsWithDetails = await _reviewService.GetReviewsWithDetailsByTrimIdAsync(review.TrimId);
            var reviewWithDetails = reviewsWithDetails.FirstOrDefault(r =>
                ((dynamic)r).Review.Id == id);

            if (reviewWithDetails == null)
                return NotFound();

            return Ok(reviewWithDetails);
        }

        /// <summary>
        /// Updates an existing review.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Update review")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] Review updatedReview)
        {
            var existingReview = await _reviewService.GetReviewByIdAsync(id);
            if (existingReview == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || existingReview.UserId != userId.Value)
                return Forbid();

            if (updatedReview.Rating < 1 || updatedReview.Rating > 10)
                return BadRequest("Рейтинг має бути в діапазоні від 1 до 10");

            updatedReview.Id = id;
            updatedReview.UserId = userId.Value;
            updatedReview.UpdatedAt = DateTime.UtcNow;

            await _reviewService.UpdateReviewAsync(id, updatedReview);

            var reviewsWithDetails = await _reviewService.GetReviewsWithDetailsByTrimIdAsync(existingReview.TrimId);
            var updatedReviewWithDetails = reviewsWithDetails.FirstOrDefault(r =>
                ((dynamic)r).Review.Id == id);

            return Ok(updatedReviewWithDetails);
        }

        /// <summary>
        /// Deletes an existing review.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Delete review")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
                return NotFound();

            var userId = GetCurrentUserId();
            if (userId == null || review.UserId != userId.Value)
                return Forbid();

            await _reviewService.DeleteReviewAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Returns reviews created by a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get reviews by user")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReviewsByUser(int userId)
        {
            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);

            var result = new List<object>();
            foreach (var review in reviews)
            {
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
                                var user = await _authService.GetUserByIdAsync(review.UserId);
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

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }
    }
}
