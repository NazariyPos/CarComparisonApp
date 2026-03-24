
using CarComparisonApi.Controllers;
using CarComparisonApi.Models.DTOs;
using CarComparisonApi.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CarComparisonApp.Tests.Controller
{
    public class CarControllerSearchTests
    {
        private readonly CarsController _controller;
        private readonly ICarService _mockCarService;

        public CarControllerSearchTests()
        {
            _mockCarService = A.Fake<ICarService>();
            _controller = new CarsController(_mockCarService);
        }

        [Fact]
        public async Task Search_ModelWithoutBrand_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: null,
                model: "Camry",
                generation: null,
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var messageProperty = response.GetType().GetProperty("message");
            var errorsProperty = response.GetType().GetProperty("errors");

            Assert.NotNull(messageProperty);
            Assert.NotNull(errorsProperty);

            var message = messageProperty.GetValue(response) as string;
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Equal("������� �������� ��������� ������", message);
            Assert.Contains("��� ������ �� ������� ��������� ������� ����� (�������� brand)", errors);
        }

        [Fact]
        public async Task Search_GenerationWithoutBrand_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: null,
                model: "Camry",
                generation: "XV70",
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var errorsProperty = response.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Contains("��� ������ �� ��������� ��������� ������� ����� (�������� brand)", errors);
        }

        [Fact]
        public async Task Search_GenerationWithoutModel_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: "Toyota",
                model: null,
                generation: "XV70",
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var errorsProperty = response.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Contains("��� ������ �� ��������� ��������� ������� ������ (�������� model)", errors);
        }

        [Fact]
        public async Task Search_MinYearGreaterThanMaxYear_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: null,
                model: null,
                generation: null,
                minYear: 2020,
                maxYear: 2010,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var errorsProperty = response.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Contains("̳�������� �� �� ���� ���� ������ �� ������������", errors);
        }

        [Fact]
        public async Task Search_MinYearLessThan1900_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: null,
                model: null,
                generation: null,
                minYear: 1899,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var errorsProperty = response.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Contains("̳�������� �� �� ���� ���� ������ �� 1900", errors);
        }

        [Fact]
        public async Task Search_MaxYearGreaterThanCurrentYearPlusOne_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Search(
                brand: null,
                model: null,
                generation: null,
                minYear: null,
                maxYear: DateTime.Now.Year + 2,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value;
            Assert.NotNull(response);

            var errorsProperty = response.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = Assert.IsAssignableFrom<IEnumerable<string>>(errorsProperty.GetValue(response));

            Assert.Contains($"������������ �� �� ���� ���� ������ �� {DateTime.Now.Year + 1}", errors);
        }

        [Fact]
        public async Task Search_ValidParameters_ServiceReturnsEmpty_ReturnsNotFound()
        {
            // Arrange
            const string brand = "Toyota";
            const string model = "Camry";

            var emptyResult = Enumerable.Empty<GenerationCardDto>();
            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, model, null, null, null, null, null, null))
                .Returns(Task.FromResult(emptyResult));

            // Act
            var result = await _controller.Search(
                brand: brand,
                model: model,
                generation: null,
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value;
            Assert.NotNull(response);

            var messageProperty = response.GetType().GetProperty("message");
            var parametersProperty = response.GetType().GetProperty("parameters");

            Assert.NotNull(messageProperty);
            Assert.NotNull(parametersProperty);

            var message = messageProperty.GetValue(response) as string;
            var parameters = parametersProperty.GetValue(response);
            Assert.NotNull(parameters);

            Assert.Equal("�� ������ ��������� �� �������� ������� �������� ����", message);

            var brandParamProperty = parameters.GetType().GetProperty("brand");
            var modelParamProperty = parameters.GetType().GetProperty("model");

            Assert.NotNull(brandParamProperty);
            Assert.NotNull(modelParamProperty);

            var brandParam = brandParamProperty.GetValue(parameters) as string;
            var modelParam = modelParamProperty.GetValue(parameters) as string;

            Assert.Equal(brand, brandParam);
            Assert.Equal(model, modelParam);
        }

        [Fact]
        public async Task Search_ValidParameters_ServiceReturnsData_ReturnsOkWithData()
        {
            // Arrange
            const string brand = "Toyota";
            const string model = "Camry";

            var expectedData = new List<GenerationCardDto>
            {
                new GenerationCardDto
                {
                    BrandId = 1,
                    BrandName = "Toyota",
                    ModelId = 1,
                    ModelName = "Camry",
                    GenerationId = 1,
                    GenerationName = "XV70",
                    YearFrom = 2017,
                    YearTo = 2024,
                    TrimCount = 2
                }
            };

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, model, null, null, null, null, null, null))
                .Returns(Task.FromResult(expectedData.AsEnumerable()));

            // Act
            var result = await _controller.Search(
                brand: brand,
                model: model,
                generation: null,
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = okResult.Value as IEnumerable<GenerationCardDto>;

            Assert.NotNull(returnedData);
            var dataList = returnedData.ToList();

            Assert.Single(dataList);
            Assert.Equal("Toyota", dataList[0].BrandName);
            Assert.Equal("Camry", dataList[0].ModelName);
            Assert.Equal("XV70", dataList[0].GenerationName);

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, model, null, null, null, null, null, null))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Search_ServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            const string brand = "Toyota";

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, null, null, null, null, null, null, null))
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _controller.Search(
                brand: brand,
                model: null,
                generation: null,
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            var response = statusCodeResult.Value;
            Assert.NotNull(response);

            var messageProperty = response.GetType().GetProperty("message");
            var errorProperty = response.GetType().GetProperty("error");

            Assert.NotNull(messageProperty);
            Assert.NotNull(errorProperty);

            var message = messageProperty.GetValue(response) as string;
            var error = errorProperty.GetValue(response) as string;

            Assert.Equal("Сталася внутрішня помилка під час пошуку", message);
            Assert.Equal("Database connection failed", error);
        }

        [Fact]
        public async Task Search_AllFiltersApplied_ServiceCalledWithCorrectParameters()
        {
            // Arrange
            const string brand = "Toyota";
            const string model = "Camry";
            const string generation = "XV70";
            const int minYear = 2015;
            const int maxYear = 2020;
            const string bodyType = "Седан";
            const string transmission = "Автомат";
            const string fuelType = "Бензин";

            var expectedData = new List<GenerationCardDto>
            {
                new GenerationCardDto { BrandName = "Toyota", ModelName = "Camry" }
            };

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, model, generation, minYear, maxYear, bodyType, transmission, fuelType))
                .Returns(Task.FromResult(expectedData.AsEnumerable()));

            // Act
            var result = await _controller.Search(
                brand: brand,
                model: model,
                generation: generation,
                minYear: minYear,
                maxYear: maxYear,
                bodyType: bodyType,
                transmission: transmission,
                fuelType: fuelType);

            // Assert
            Assert.IsType<OkObjectResult>(result);

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                brand, model, generation, minYear, maxYear, bodyType, transmission, fuelType))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Search_NoParameters_ReturnsAllResults()
        {
            // Arrange
            var expectedData = new List<GenerationCardDto>
            {
                new GenerationCardDto { BrandName = "Toyota" },
                new GenerationCardDto { BrandName = "Honda" }
            };

            A.CallTo(() => _mockCarService.GetGenerationCardsAsync(
                null, null, null, null, null, null, null, null))
                .Returns(Task.FromResult(expectedData.AsEnumerable()));

            // Act
            var result = await _controller.Search(
                brand: null,
                model: null,
                generation: null,
                minYear: null,
                maxYear: null,
                bodyType: null,
                transmission: null,
                fuelType: null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = okResult.Value as IEnumerable<GenerationCardDto>;

            Assert.NotNull(returnedData);
            Assert.Equal(2, returnedData.Count());
        }
    }
}
