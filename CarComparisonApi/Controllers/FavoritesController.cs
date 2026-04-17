using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Provides endpoints for managing current user's favorite trims.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        public FavoritesController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Returns current user's favorites list.
        /// </summary>
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user favorites")]
        [ProducesResponseType(typeof(IEnumerable<FavoriteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var favorites = await _favoriteService.GetUserFavoritesAsync(userId.Value);
            return Ok(favorites);
        }

        /// <summary>
        /// Adds trim to current user's favorites.
        /// </summary>
        [HttpPost]
        [SwaggerOperation(Summary = "Add trim to favorites")]
        [ProducesResponseType(typeof(FavoriteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest request)
        {
            if (request.TrimId <= 0)
            {
                return BadRequest("ID комплектації має бути додатним числом");
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var favorite = await _favoriteService.AddFavoriteAsync(userId.Value, request.TrimId);
            if (favorite == null)
            {
                return NotFound($"Комплектація з ID {request.TrimId} не знайдена");
            }

            return CreatedAtAction(nameof(GetMyFavorites), new { }, favorite);
        }

        /// <summary>
        /// Removes trim from current user's favorites.
        /// </summary>
        [HttpDelete("trim/{trimId:int}")]
        [SwaggerOperation(Summary = "Remove trim from favorites")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFavorite(int trimId)
        {
            if (trimId <= 0)
            {
                return BadRequest("ID комплектації має бути додатним числом");
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var removed = await _favoriteService.RemoveFavoriteAsync(userId.Value, trimId);
            if (!removed)
            {
                return NotFound("Запис в обраному не знайдено");
            }

            return NoContent();
        }

        /// <summary>
        /// Checks whether trim is in current user's favorites.
        /// </summary>
        [HttpGet("trim/{trimId:int}/exists")]
        [SwaggerOperation(Summary = "Check if trim is favorite")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> IsFavorite(int trimId)
        {
            if (trimId <= 0)
            {
                return BadRequest("ID комплектації має бути додатним числом");
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var exists = await _favoriteService.IsFavoriteAsync(userId.Value, trimId);
            return Ok(new { TrimId = trimId, IsFavorite = exists });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}
