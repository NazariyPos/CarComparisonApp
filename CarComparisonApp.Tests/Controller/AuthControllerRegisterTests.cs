using CarComparisonApi.Controllers;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApp.Tests.Controller
{
    public class AuthControllerRegisterTests
    {
        private readonly AuthController _controller;
        private readonly IAuthService _mockAuthService;

        public AuthControllerRegisterTests()
        {
            _mockAuthService = A.Fake<IAuthService>();
            _controller = new AuthController(_mockAuthService);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOkWithAuthResponse()
        {
            // Arrange
            var validRequest = new RegisterRequest
            {
                Login = "valid_user123",
                Password = "ValidPass123",
                Email = "test@example.com",
                RealName = "John Doe"
            };

            var expectedResponse = new AuthResponse
            {
                Token = "jwt-token-here",
                User = new UserDto
                {
                    Id = 1,
                    Login = "valid_user123",
                    Username = "NewUser1",
                    Email = "test@example.com",
                    RealName = "John Doe",
                    IsAdmin = false
                }
            };

            A.CallTo(() => _mockAuthService.RegisterAsync(validRequest))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.Register(validRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as AuthResponse;

            Assert.NotNull(response);
            Assert.Equal("jwt-token-here", response.Token);
            Assert.Equal("valid_user123", response.User.Login);
            Assert.Equal("NewUser1", response.User.Username);

            A.CallTo(() => _mockAuthService.RegisterAsync(validRequest))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Register_LoginTooLong_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "this_login_is_way_too_long_for_validation",
                Password = "ValidPass123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Логін має бути не більше 20 символів", messageValue);

            A.CallTo(() => _mockAuthService.RegisterAsync(A<RegisterRequest>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Register_LoginWithInvalidCharacters_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "user@name",
                Password = "ValidPass123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Логін має містити тільки латинські літери, цифри та знак підкреслення", messageValue);
        }

        [Fact]
        public async Task Register_PasswordTooShort_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "validuser",
                Password = "Short1"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Пароль має бути не менше 8 символів", messageValue);
        }

        [Fact]
        public async Task Register_PasswordNoDigits_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "validuser",
                Password = "NoDigitsHere"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Пароль має містити принаймні одну велику літеру, одну малу літеру та одну цифру", messageValue);
        }

        [Fact]
        public async Task Register_PasswordNoLowercase_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "validuser",
                Password = "UPPERCASE123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Пароль має містити принаймні одну велику літеру, одну малу літеру та одну цифру", messageValue);
        }

        [Fact]
        public async Task Register_PasswordNoUppercase_ReturnsBadRequest()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Login = "validuser",
                Password = "lowercase123"
            };

            // Act
            var result = await _controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal("Пароль має містити принаймні одну велику літеру, одну малу літеру та одну цифру", messageValue);
        }

        [Fact]
        public async Task Register_ServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            var validRequest = new RegisterRequest
            {
                Login = "valid_user",
                Password = "ValidPass123",
                Email = "test@example.com"
            };

            const string exceptionMessage = "Користувач з таким логіном або email вже існує";
            A.CallTo(() => _mockAuthService.RegisterAsync(validRequest))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.Register(validRequest);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var responseType = response.GetType();

            var successProperty = responseType.GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = Assert.IsType<bool>(successProperty.GetValue(response));
            Assert.False(successValue);

            var messageProperty = responseType.GetProperty("message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(response) as string;
            Assert.Equal(exceptionMessage, messageValue);
        }
    }
}
