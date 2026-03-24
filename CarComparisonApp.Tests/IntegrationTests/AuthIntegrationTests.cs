using CarComparisonApi.Controllers;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CarComparisonApp.Tests.IntegrationTests
{
    public class AuthIntegrationTests : IDisposable
    {
        private readonly string _originalUsersFilePath;
        private readonly string _backupUsersFilePath;
        private readonly JsonUserService _userService;
        private readonly AuthService _authService;
        private readonly AuthController _authController;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AuthIntegrationTests()
        {
            _originalUsersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "CarComparisonApi", "Data", "users.json");

            var usersDirectory = Path.GetDirectoryName(_originalUsersFilePath)
                ?? throw new InvalidOperationException("Cannot determine users file directory.");
            var projectRoot = Directory.GetParent(usersDirectory)?.FullName
                ?? throw new InvalidOperationException("Cannot determine project root directory.");

            Console.WriteLine($"Project root: {projectRoot}");
            Console.WriteLine($"Original file path: {_originalUsersFilePath}");

            _backupUsersFilePath = Path.Combine(Path.GetTempPath(), $"users_backup_{Guid.NewGuid()}.json");

            if (File.Exists(_originalUsersFilePath))
            {
                File.Copy(_originalUsersFilePath, _backupUsersFilePath, true);

                var originalContent = File.ReadAllText(_originalUsersFilePath);
                var initialUsers = JsonConvert.DeserializeObject<List<User>>(originalContent) ?? new List<User>();

                AddTestUsersIfNotExist(initialUsers, _originalUsersFilePath);
            }
            else
            {
                var initialUsers = new List<User>
                {
                    new User
                    {
                        Id = 1,
                        Login = "admin",
                        Username = "Admin",
                        Email = "admin@example.com",
                        PasswordHash = "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=",
                        IsAdmin = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = 2,
                        Login = "testuser",
                        Username = "TestUser",
                        Email = "testmail@gmail.com",
                        PasswordHash = "oQnjaUetVt4dyhzEnw74rJrZp7GqDfQfs8TLc8H/Aeo=",
                        IsAdmin = false,
                        RealName = "Тестовий Користувач",
                        CreatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Id = 3,
                        Login = "testuser001",
                        Username = "NewUser1",
                        Email = "testuser001@example.com",
                        PasswordHash = "y+51Iw1Eug2yDMO84NvIcuZpZn/ddeiNObtMaKvKF9Q=",
                        IsAdmin = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                var directory = Path.GetDirectoryName(_originalUsersFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                File.WriteAllText(_originalUsersFilePath, JsonConvert.SerializeObject(initialUsers, Formatting.Indented));
            }

            _environment = A.Fake<IWebHostEnvironment>();
            A.CallTo(() => _environment.ContentRootPath).Returns(projectRoot);

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "your-super-secret-key-32-chars-long-here!",
                    ["Jwt:Issuer"] = "CarComparisonApi",
                    ["Jwt:Audience"] = "CarComparisonApiUsers",
                    ["Jwt:ExpireDays"] = "7"
                });
            _configuration = configurationBuilder.Build();

            _userService = new JsonUserService(_environment);
            _authService = new AuthService(_configuration, _userService, _environment);
            _authController = new AuthController(_authService);

            Console.WriteLine($"ContentRootPath: {_environment.ContentRootPath}");
            Console.WriteLine($"Expected file path: {Path.Combine(_environment.ContentRootPath, "Data", "users.json")}");
        }

        private static void AddTestUsersIfNotExist(List<User> existingUsers, string filePath)
        {
            bool needsUpdate = false;

            if (!existingUsers.Any(u => u.Email == "testmail@gmail.com"))
            {
                var testUser = new User
                {
                    Id = existingUsers.Count > 0 ? existingUsers.Max(u => u.Id) + 1 : 1,
                    Login = "testuser",
                    Username = "TestUser",
                    Email = "testmail@gmail.com",
                    PasswordHash = "oQnjaUetVt4dyhzEnw74rJrZp7GqDfQfs8TLc8H/Aeo=",
                    IsAdmin = false,
                    RealName = "Тестовий Користувач",
                    CreatedAt = DateTime.UtcNow
                };
                existingUsers.Add(testUser);
                needsUpdate = true;
            }

            if (!existingUsers.Any(u => u.Login == "admin"))
            {
                var adminUser = new User
                {
                    Id = existingUsers.Count > 0 ? existingUsers.Max(u => u.Id) + 1 : 1,
                    Login = "admin",
                    Username = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };
                existingUsers.Add(adminUser);
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(existingUsers, Formatting.Indented));
            }
        }

        public void Dispose()
        {
            try
            {
                if (File.Exists(_backupUsersFilePath))
                {
                    File.Copy(_backupUsersFilePath, _originalUsersFilePath, true);
                    File.Delete(_backupUsersFilePath);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Помилка при відновленні файлу: {ex.Message}");
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }

        [Fact]
        public async Task Register_ValidData_ReturnsSuccessAndCreatesUserInJsonFile()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "newuser123",
                Email = "newuser123@example.com",
                Password = "SecurePass123",
                RealName = "Іван Іванов"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);

            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var allUsers = await _userService.GetAllUsersAsync();
            var savedUser = allUsers.FirstOrDefault(u => u.Login == "newuser123");

            Assert.NotNull(savedUser);
            Assert.Equal("newuser123@example.com", savedUser.Email);
            Assert.Equal("Іван Іванов", savedUser.RealName);
            Assert.NotEqual("SecurePass123", savedUser.PasswordHash);
            Assert.True(savedUser.PasswordHash.Length > 20);
            Assert.StartsWith("NewUser", savedUser.Username);
        }

        [Fact]
        public async Task Register_WithLoginMoreThan20Characters_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "thisloginismorethantwentycharacters",
                Email = "test@example.com",
                Password = "SecurePass123"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Логін має бути не більше 20 символів", response);
        }

        [Fact]
        public async Task Register_WithInvalidLoginFormat_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "user@test",
                Email = "test@example.com",
                Password = "SecurePass123"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Логін має містити тільки латинські літери, цифри та знак підкреслення", response);
        }

        [Fact]
        public async Task Register_WithPasswordLessThan8Characters_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "testuser",
                Email = "test@example.com",
                Password = "Short1"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Пароль має бути не менше 8 символів", response);
        }

        [Fact]
        public async Task Register_WithPasswordWithoutUppercase_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "testuser",
                Email = "test@example.com",
                Password = "lowercase123"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Пароль має містити принаймні одну велику літеру", response);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "differentlogin",
                Email = "testmail@gmail.com",
                Password = "SecurePass123"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Користувач з таким логіном або email вже існує", response);
        }

        [Fact]
        public async Task Register_WithDuplicateLogin_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "testuser",
                Email = "newemail@example.com",
                Password = "SecurePass123"
            };

            // Act
            var result = await _authController.Register(request);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>(result);

            var badRequestResult = result as Microsoft.AspNetCore.Mvc.BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = JsonConvert.SerializeObject(badRequestResult.Value);
            Assert.Contains("Користувач з таким логіном або email вже існує", response);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Login = "loginuser",
                Email = "loginuser@example.com",
                Password = "SecurePass123"
            };

            await _authController.Register(registerRequest);

            // Act
            var loginRequest = new LoginRequest
            {
                LoginOrEmail = "loginuser",
                Password = "SecurePass123"
            };

            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);

            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var response = JsonConvert.SerializeObject(okResult.Value);
            Assert.Contains("\"Token\"", response);
            Assert.Contains("\"User\"", response);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                LoginOrEmail = "testuser",
                Password = "WrongPassword123"
            };

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult>(result);

            var unauthorizedResult = result as Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var response = JsonConvert.SerializeObject(unauthorizedResult.Value);
            Assert.Contains("Неправильний логін або пароль", response);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                LoginOrEmail = "nonexistentuser",
                Password = "SomePassword123"
            };

            // Act
            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult>(result);

            var unauthorizedResult = result as Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(401, unauthorizedResult.StatusCode);

            var response = JsonConvert.SerializeObject(unauthorizedResult.Value);
            Assert.Contains("Неправильний логін або пароль", response);
        }

        [Fact]
        public async Task Login_WithEmailInsteadOfLogin_ReturnsToken()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Login = "emaillogin",
                Email = "emaillogin@example.com",
                Password = "SecurePass123"
            };

            await _authController.Register(registerRequest);

            // Act
            var loginRequest = new LoginRequest
            {
                LoginOrEmail = "emaillogin@example.com",
                Password = "SecurePass123"
            };

            var result = await _authController.Login(loginRequest);

            // Assert
            Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);

            var okResult = result as Microsoft.AspNetCore.Mvc.OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);

            var response = JsonConvert.SerializeObject(okResult.Value);
            Assert.Contains("\"Token\"", response);
        }
    }
}
