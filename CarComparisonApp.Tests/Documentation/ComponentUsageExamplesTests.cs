using CarComparisonApi.Controllers;
using CarComparisonApi.Models;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApp.Tests.Documentation
{
    public class ComponentUsageExamplesTests
    {
        [Fact]
        public async Task Example_Register_User_With_AuthController()
        {
            var authService = A.Fake<IAuthService>();
            var controller = new AuthController(authService);

            var request = new RegisterRequest
            {
                Login = "doc_user",
                Email = "doc_user@example.com",
                Password = "SecurePass123",
                RealName = "Doc User"
            };

            var expected = new AuthResponse
            {
                Token = "example-token",
                User = new UserDto
                {
                    Id = 10,
                    Login = request.Login,
                    Username = "NewUser10",
                    Email = request.Email,
                    RealName = request.RealName,
                    IsAdmin = false
                }
            };

            A.CallTo(() => authService.RegisterAsync(request)).Returns(expected);

            var result = await controller.Register(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(ok.Value);
            Assert.Equal("example-token", response.Token);
            Assert.Equal("doc_user", response.User.Login);
        }

        [Fact]
        public async Task Example_Search_Generations_With_CarsController()
        {
            var carService = A.Fake<ICarService>();
            var controller = new CarsController(carService);

            var cards = new List<GenerationCardDto>
            {
                new()
                {
                    BrandId = 1,
                    BrandName = "Toyota",
                    ModelId = 1,
                    ModelName = "Camry",
                    GenerationId = 100,
                    GenerationName = "XV70",
                    BodyType = "Sedan",
                    YearFrom = 2017,
                    YearTo = 2024,
                    TrimCount = 3
                }
            };

            A.CallTo(() => carService.GetGenerationCardsAsync("Toyota", "Camry", null, null, null, null, null, null))
                .Returns(Task.FromResult(cards.AsEnumerable()));

            var result = await controller.Search("Toyota", "Camry", null, null, null, null, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsAssignableFrom<IEnumerable<GenerationCardDto>>(ok.Value);
            Assert.Single(payload);
        }

        [Fact]
        public async Task Example_Compare_Trims_With_ComparisonController()
        {
            var carService = A.Fake<ICarService>();
            var controller = new ComparisonController(carService);

            var trims = new List<Trim>
            {
                new()
                {
                    Id = 1,
                    Name = "2.0 MT",
                    TechnicalDetails = new TechnicalDetails
                    {
                        MaxSpeed = 210,
                        Acceleration0To100 = 8.5m,
                        Power = 180,
                        Torque = 250,
                        FuelConsumptionMixed = 7.2m
                    }
                },
                new()
                {
                    Id = 2,
                    Name = "2.5 AT",
                    TechnicalDetails = new TechnicalDetails
                    {
                        MaxSpeed = 230,
                        Acceleration0To100 = 7.6m,
                        Power = 220,
                        Torque = 320,
                        FuelConsumptionMixed = 8.1m
                    }
                }
            };

            A.CallTo(() => carService.GetTrimsForComparisonAsync(A<IEnumerable<int>>._))
                .Returns(Task.FromResult(trims.AsEnumerable()));

            var result = await controller.Compare("1,2");

            Assert.IsType<OkObjectResult>(result);
        }
    }
}
