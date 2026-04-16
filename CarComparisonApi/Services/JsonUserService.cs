// CarComparisonApi/Services/JsonUserService.cs
using CarComparisonApi.Data;
using CarComparisonApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Contract for user storage operations.
    /// </summary>
    public interface IJsonUserService
    {
        /// <summary>
        /// Returns all users from storage.
        /// </summary>
        /// <returns>List of users.</returns>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// Returns a user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// Returns a user by login.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        Task<User?> GetUserByLoginAsync(string login);

        /// <summary>
        /// Returns a user by login or email.
        /// </summary>
        /// <param name="loginOrEmail">Login or email value.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        Task<User?> GetUserByLoginOrEmailAsync(string loginOrEmail);

        /// <summary>
        /// Checks whether a user with the specified login or email already exists.
        /// </summary>
        /// <param name="login">Login to check.</param>
        /// <param name="email">Email to check.</param>
        /// <returns><c>true</c> if user exists; otherwise <c>false</c>.</returns>
        Task<bool> UserExistsAsync(string login, string email);

        /// <summary>
        /// Creates a user record.
        /// </summary>
        /// <param name="user">User data to create.</param>
        Task CreateUserAsync(User user);

        /// <summary>
        /// Updates a user record.
        /// </summary>
        /// <param name="user">User data to update.</param>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        Task DeleteUserAsync(int id);
    }

    /// <summary>
    /// SQL-backed implementation of user storage service.
    /// </summary>
    public class JsonUserService : IJsonUserService
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly ILogger<JsonUserService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonUserService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="logger">The logger.</param>
        public JsonUserService(CarComparisonDbContext dbContext, ILogger<JsonUserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Returns all users.
        /// </summary>
        /// <returns>List of users from the database.</returns>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Returns user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Returns user by login.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Login == login);
        }

        /// <summary>
        /// Returns user by login or email.
        /// </summary>
        /// <param name="loginOrEmail">Login or email value.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public async Task<User?> GetUserByLoginOrEmailAsync(string loginOrEmail)
        {
            return await _dbContext.Users
                .FirstOrDefaultAsync(u =>
                    u.Login == loginOrEmail || u.Email == loginOrEmail);
        }

        /// <summary>
        /// Checks whether login or email is already used.
        /// </summary>
        /// <param name="login">Login value.</param>
        /// <param name="email">Email value.</param>
        /// <returns><c>true</c> if user exists; otherwise <c>false</c>.</returns>
        public async Task<bool> UserExistsAsync(string login, string email)
        {
            return await _dbContext.Users
                .AnyAsync(u => u.Login == login || u.Email == email);
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">User data to persist.</param>
        public async Task CreateUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User created. UserId: {UserId}, Login: {Login}", user.Id, user.Login);
        }

        /// <summary>
        /// Updates existing user.
        /// </summary>
        /// <param name="user">User data to persist.</param>
        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null)
            {
                _logger.LogWarning("Attempted to update non-existing user. UserId: {UserId}", user.Id);
                return;
            }

            existingUser.Login = user.Login;
            existingUser.Email = user.Email;
            existingUser.PasswordHash = user.PasswordHash;
            existingUser.Username = user.Username;
            existingUser.IsAdmin = user.IsAdmin;
            existingUser.RealName = user.RealName;
            existingUser.About = user.About;
            existingUser.AvatarUrl = user.AvatarUrl;
            existingUser.CreatedAt = user.CreatedAt;
            existingUser.LastLogin = user.LastLogin;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User updated. UserId: {UserId}", user.Id);
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        public async Task DeleteUserAsync(int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                _logger.LogWarning("Attempted to delete non-existing user. UserId: {UserId}", id);
                return;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User deleted. UserId: {UserId}", id);
        }
    }
}
