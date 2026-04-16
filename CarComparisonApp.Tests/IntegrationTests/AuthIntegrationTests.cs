using CarComparisonApi.Controllers;
using CarComparisonApi.Data;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CarComparisonApp.Tests.IntegrationTests
{
    public class AuthIntegrationTests : IDisposable
    {
        private readonly CarComparisonDbContext _dbContext;
        private readonly JsonUserService _userService;
        private readonly AuthService _authService;
        private readonly AuthController _authController;

        public AuthIntegrationTests()
        {
            var dbOptions = new DbContextOptionsBuilder<CarComparisonDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new CarComparisonDbContext(dbOptions);
            SeedUsers(_dbContext);

            var configurationBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = "your-super-secret-key-32-chars-long-here!",
                    ["Jwt:Issuer"] = "CarComparisonApi",
                    ["Jwt:Audience"] = "CarComparisonApiUsers",
                    ["Jwt:ExpireDays"] = "7"
                });

            var configuration = configurationBuilder.Build();
            _userService = new JsonUserService(_dbContext, NullLogger<JsonUserService>.Instance);
            _authService = new AuthService(configuration, _userService, NullLogger<AuthService>.Instance);
            _authController = new AuthController(_authService);
        }

        private static void SeedUsers(CarComparisonDbContext dbContext)
        {
            dbContext.Users.AddRange(
                new User
                {
                    Login = "admin",
                    Username = "Admin",
                    Email = "admin@example.com",
                    PasswordHash = "PrP+ZrMeO00Q+nC1ytSccRIpSvauTkdqHEBRVdRaoSE=",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
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
                    Login = "testuser001",
                    Username = "NewUser1",
                    Email = "testuser001@example.com",
                    PasswordHash = "y+51Iw1Eug2yDMO84NvIcuZpZn/ddeiNObtMaKvKF9Q=",
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                });

            dbContext.SaveChanges();
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
            GC.SuppressFinalize(this);
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
