using CarComparisonApi.Controllers;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;

namespace CarComparisonApp.Tests.Controller
{
    public class CarsControllerGetGenerationDetailsTests
    {
        private readonly CarsController _controller;
        private readonly ICarService _mockCarService;

        public CarsControllerGetGenerationDetailsTests()
        {
            _mockCarService = A.Fake<ICarService>();
            _controller = new CarsController(_mockCarService);
        }

        [Fact]
        public async Task GetGenerationDetails_ValidId_ReturnsOkWithGeneration()
        {
            // Arrange
            const int generationId = 1;

            var generationDto = new GenerationWithTrimsDto
            {
                Id = generationId,
                Name = "XV70",
                YearFrom = 2017,
                YearTo = 2024,
                PhotoUrl = "/photos/toyota-camry-xv70.jpg",
                Brand = new BrandDto
                {
                    Id = 1,
                    Name = "Toyota"
                },
                Model = new ModelDto
                {
                    Id = 1,
                    Name = "Camry",
                    BodyType = "Седан",
                    BrandId = 1
                },
                Trims = new List<TrimBasicDto>
                {
                    new TrimBasicDto
                    {
                        Id = 1,
                        Name = "LE 2.5",
                        TransmissionType = "Автомат",
                        DoorsCount = 4,
                        SeatsCount = 5
                    },
                    new TrimBasicDto
                    {
                        Id = 2,
                        Name = "Hybrid",
                        TransmissionType = "Варіатор",
                        DoorsCount = 4,
                        SeatsCount = 5
                    }
                }
            };

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(generationId))
                .Returns(Task.FromResult<GenerationWithTrimsDto?>(generationDto));

            // Act
            var result = await _controller.GetGenerationDetails(generationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedGeneration = okResult.Value as GenerationWithTrimsDto;

            Assert.NotNull(returnedGeneration);
            Assert.Equal(generationId, returnedGeneration.Id);
            Assert.Equal("XV70", returnedGeneration.Name);
            Assert.Equal(2017, returnedGeneration.YearFrom);
            Assert.Equal(2024, returnedGeneration.YearTo);
            Assert.Equal("Toyota", returnedGeneration.Brand.Name);
            Assert.Equal("Camry", returnedGeneration.Model.Name);
            Assert.Equal(2, returnedGeneration.Trims.Count);
            Assert.Contains(returnedGeneration.Trims, t => t.Name == "LE 2.5");
            Assert.Contains(returnedGeneration.Trims, t => t.Name == "Hybrid");

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(generationId))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task GetGenerationDetails_ZeroId_ReturnsBadRequest()
        {
            // Arrange
            const int zeroId = 0;

            // Act
            var result = await _controller.GetGenerationDetails(zeroId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID має бути додатним числом", badRequestResult.Value);

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(A<int>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task GetGenerationDetails_NegativeId_ReturnsBadRequest()
        {
            // Arrange
            const int negativeId = -5;

            // Act
            var result = await _controller.GetGenerationDetails(negativeId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID має бути додатним числом", badRequestResult.Value);

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(A<int>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task GetGenerationDetails_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            const int nonExistingId = 999;

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(nonExistingId))
                .Returns(Task.FromResult<GenerationWithTrimsDto?>(null));

            // Act
            var result = await _controller.GetGenerationDetails(nonExistingId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal($"Покоління з ID {nonExistingId} не знайдено", notFoundResult.Value);

            A.CallTo(() => _mockCarService.GetGenerationWithTrimsAsync(nonExistingId))
                .MustHaveHappenedOnceExactly();
        }
    }
}
