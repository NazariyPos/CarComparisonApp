using CarComparisonApi.Models;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
                return NotFound();

            return Ok(new
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                RealName = user.RealName,
                About = user.About,
                AvatarUrl = user.AvatarUrl
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var user = await _authService.GetUserByIdAsync(userId.Value);
            if (user == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.RealName))
                user.RealName = request.RealName;

            if (!string.IsNullOrEmpty(request.About))
                user.About = request.About;

            if (!string.IsNullOrEmpty(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            return Ok(new { message = "Профіль оновлено" });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                return userId;
            return null;
        }
    }

    public class UpdateProfileRequest
    {
        public string? RealName { get; set; }
        public string? About { get; set; }
        public string? AvatarUrl { get; set; }
    }
}