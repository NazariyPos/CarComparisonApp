using System;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CarComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
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

        [HttpPost("login")]
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

        [HttpGet("me")]
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