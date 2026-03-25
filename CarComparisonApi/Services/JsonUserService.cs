// CarComparisonApi/Services/JsonUserService.cs
using CarComparisonApi.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace CarComparisonApi.Services
{
    /// <summary>
    /// Contract for JSON-based user storage operations.
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
    /// JSON-file implementation of user storage service.
    /// </summary>
    public class JsonUserService : IJsonUserService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _usersFilePath;
        private List<User> _users = new();
        private readonly object _lock = new();

        /// <summary>
        /// Initializes JSON user storage.
        /// </summary>
        /// <param name="environment">Host environment used to resolve data file path.</param>
        public JsonUserService(IWebHostEnvironment environment)
        {
            _environment = environment;
            _usersFilePath = Path.Combine(environment.ContentRootPath, "Data", "users.json");
            LoadUsers();
        }

        private void LoadUsers()
        {
            lock (_lock)
            {
                if (File.Exists(_usersFilePath))
                {
                    var json = File.ReadAllText(_usersFilePath);
                    _users = JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
                }
                else
                {
                    _users = new List<User>
                    {
                        new User
                        {
                            Id = 1,
                            Login = "admin",
                            Username = "Admin",
                            Email = "admin@example.com",
                            PasswordHash = "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=", // admin123
                            IsAdmin = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };
                    SaveUsers();
                }
            }
        }

        private void SaveUsers()
        {
            var json = JsonConvert.SerializeObject(_users, Formatting.Indented);
            File.WriteAllText(_usersFilePath, json);
        }

        /// <summary>
        /// Returns all users.
        /// </summary>
        /// <returns>List of users from the JSON store.</returns>
        public Task<List<User>> GetAllUsersAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_users.ToList());
            }
        }

        /// <summary>
        /// Returns user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public Task<User?> GetUserByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
            }
        }

        /// <summary>
        /// Returns user by login.
        /// </summary>
        /// <param name="login">User login.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public Task<User?> GetUserByLoginAsync(string login)
        {
            lock (_lock)
            {
                return Task.FromResult(_users.FirstOrDefault(u => u.Login == login));
            }
        }

        /// <summary>
        /// Returns user by login or email.
        /// </summary>
        /// <param name="loginOrEmail">Login or email value.</param>
        /// <returns>User instance or <c>null</c> if not found.</returns>
        public Task<User?> GetUserByLoginOrEmailAsync(string loginOrEmail)
        {
            lock (_lock)
            {
                return Task.FromResult(_users.FirstOrDefault(u =>
                    u.Login == loginOrEmail || u.Email == loginOrEmail));
            }
        }

        /// <summary>
        /// Checks whether login or email is already used.
        /// </summary>
        /// <param name="login">Login value.</param>
        /// <param name="email">Email value.</param>
        /// <returns><c>true</c> if user exists; otherwise <c>false</c>.</returns>
        public Task<bool> UserExistsAsync(string login, string email)
        {
            lock (_lock)
            {
                return Task.FromResult(_users.Any(u => u.Login == login || u.Email == email));
            }
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user">User data to persist.</param>
        public Task CreateUserAsync(User user)
        {
            lock (_lock)
            {
                user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
                _users.Add(user);
                SaveUsers();
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Updates existing user.
        /// </summary>
        /// <param name="user">User data to persist.</param>
        public Task UpdateUserAsync(User user)
        {
            lock (_lock)
            {
                var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
                if (existingUser != null)
                {
                    var index = _users.IndexOf(existingUser);
                    _users[index] = user;
                    SaveUsers();
                }
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Deletes a user by identifier.
        /// </summary>
        /// <param name="id">User identifier.</param>
        public Task DeleteUserAsync(int id)
        {
            lock (_lock)
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    _users.Remove(user);
                    SaveUsers();
                }
                return Task.CompletedTask;
            }
        }
    }
}
