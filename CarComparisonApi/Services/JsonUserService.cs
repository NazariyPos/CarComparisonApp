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
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByLoginAsync(string login);
        Task<User?> GetUserByLoginOrEmailAsync(string loginOrEmail);
        Task<bool> UserExistsAsync(string login, string email);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
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
