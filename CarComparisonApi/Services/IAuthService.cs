using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Provides user authentication and identity lookup operations.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">Registration payload.</param>
        /// <returns>Authentication response with token and user data.</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Authenticates an existing user.
        /// </summary>
        /// <param name="request">Login payload.</param>
        /// <returns>Authentication response with token and user data.</returns>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Gets a user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        Task<User?> GetUserByIdAsync(int id);
    }
}
