using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CarComparisonApi.Controllers
{
    /// <summary>
    /// Handles user registration, authentication and retrieval of current user profile.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user and returns a JWT token with profile data.
        /// </summary>
        /// <param name="request">Registration payload with login, email and password.</param>
        /// <returns>Authentication result with token and user DTO.</returns>
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Register a new user")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (request.Login.Length > 20)
                    return BadRequest(new { success = false, message = "Логін має бути не більше 20 символів" });

                if (!MyRegex().IsMatch(request.Login))
                    return BadRequest(new { success = false, message = "Логін має містити тільки латинські літери, цифри та знак підкреслення" });

                if (request.Password.Length < 8)
                    return BadRequest(new { success = false, message = "Пароль має бути не менше 8 символів" });

                if (!MyRegex1().IsMatch(request.Password))
                    return BadRequest(new { success = false, message = "Пароль має містити принаймні одну велику літеру, одну малу літеру та одну цифру" });

                var response = await _authService.RegisterAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Authenticates a user by login/email and password.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>Authentication result with JWT token.</returns>
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Sign in and get JWT token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Returns profile information for the currently authenticated user.
        /// </summary>
        /// <returns>Current user profile or an authorization error.</returns>
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                Console.WriteLine("=== GetCurrentUser called ===");
                Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");

                var userId = GetCurrentUserId();
                Console.WriteLine($"GetCurrentUserId returned: {userId}");

                if (userId == null)
                {
                    Console.WriteLine("Returning Unauthorized - userId is null");
                    return Unauthorized();
                }

                var user = await _authService.GetUserByIdAsync(userId.Value);
                if (user == null)
                {
                    Console.WriteLine($"User with id {userId} not found");
                    return NotFound();
                }

                Console.WriteLine($"User found: {user.Login}");

                return Ok(new
                {
                    Id = user.Id,
                    Login = user.Login,
                    Username = user.Username,
                    Email = user.Email,
                    IsAdmin = user.IsAdmin,
                    RealName = user.RealName,
                    About = user.About,
                    AvatarUrl = user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetCurrentUser: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst("nameid") ??
                             User.FindFirst("sub") ??
                             User.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

        [System.Text.RegularExpressions.GeneratedRegex("^[a-zA-Z0-9_]+$")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
        [System.Text.RegularExpressions.GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")]
        private static partial System.Text.RegularExpressions.Regex MyRegex1();
    }
}
