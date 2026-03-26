using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Newtonsoft.Json;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Implements registration, login and JWT token generation.
    /// </summary>
    /// <remarks>
    /// Users are stored via <see cref="IJsonUserService"/> and access tokens are signed
    /// with symmetric key settings from configuration.
    /// </remarks>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IJsonUserService _userService;

        /// <summary>
        /// Initializes authentication service dependencies.
        /// </summary>
        /// <param name="configuration">Application configuration with JWT settings.</param>
        /// <param name="userService">User storage service.</param>
        public AuthService(IConfiguration configuration, IJsonUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        /// <summary>
        /// Creates a new user account and returns authentication payload.
        /// </summary>
        /// <param name="request">Registration payload.</param>
        /// <returns>Authentication response with JWT token and user profile data.</returns>
        /// <exception cref="Exception">Thrown when user with same login or email already exists.</exception>
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userService.UserExistsAsync(request.Login, request.Email))
                throw new Exception("Користувач з таким логіном або email вже існує");

            string username = await GenerateUniqueUsernameAsync();

            var user = new User
            {
                Login = request.Login,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Username = username,
                RealName = request.RealName,
                CreatedAt = DateTime.UtcNow
            };

            await _userService.CreateUserAsync(user);

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Login = user.Login,
                    Username = user.Username,
                    Email = user.Email,
                    RealName = user.RealName,
                    IsAdmin = user.IsAdmin
                }
            };
        }

        /// <summary>
        /// Authenticates user credentials and returns authentication payload.
        /// </summary>
        /// <param name="request">Login payload.</param>
        /// <returns>Authentication response with JWT token and user profile data.</returns>
        /// <exception cref="Exception">Thrown when credentials are invalid.</exception>
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userService.GetUserByLoginOrEmailAsync(request.LoginOrEmail);
            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                throw new Exception("Неправильний логін або пароль");

            user.LastLogin = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user);

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Login = user.Login,
                    Username = user.Username,
                    Email = user.Email,
                    RealName = user.RealName,
                    IsAdmin = user.IsAdmin,
                    About = user.About,
                    AvatarUrl = user.AvatarUrl
                }
            };
        }

        private async Task<string> GenerateUniqueUsernameAsync()
        {
            var allUsers = await _userService.GetAllUsersAsync();
            int maxNumber = 0;

            foreach (var user in allUsers)
            {
                if (user.Username.StartsWith("NewUser") && int.TryParse(user.Username.AsSpan(7), out int number))
                {
                    if (number > maxNumber)
                        maxNumber = number;
                }
            }

            return $"NewUser{maxNumber + 1}";
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "your-super-secret-key-32-chars-long-here!"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string password, string passwordHash)
        {
            var hash = HashPassword(password);
            return hash == passwordHash;
        }

        /// <summary>
        /// Returns user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>User instance or <c>null</c> if not found or an error occurs.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                Console.WriteLine($"GetUserByIdAsync called with id: {id}");
                var user = await _userService.GetUserByIdAsync(id);
                Console.WriteLine($"User found: {user != null}");
                return user;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserByIdAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns user by login.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            return await _userService.GetUserByLoginAsync(login);
        }
    }
}
