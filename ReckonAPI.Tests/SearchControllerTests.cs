using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ReckonAPI.Controllers;
using ReckonAPI.Models;
using ReckonAPI.Services;
using Xunit;

namespace ReckonAPI.Tests
{
    public class SearchControllerTests
    {
        [Fact]
        public async Task PerformSearch_ValidInput_ReturnsResults()
        {
            // Arrange
            var apiServiceMock = new Mock<IApiService>();
            var loggerMock = new Mock<ILogger<SearchController>>();

            var textToSearch = new TextToSearch { text = "Sample text for searching" };
            var subTexts = new SubTexts { subTexts = new List<string> { "Sample", "text" } };

            apiServiceMock.Setup(x => x.GetTextToSearchAsync()).ReturnsAsync(textToSearch);
            apiServiceMock.Setup(x => x.GetSubTextsAsync()).ReturnsAsync(subTexts);
            apiServiceMock.Setup(x => x.PostResultsAsync(It.IsAny<SearchResultPayload>())).Returns(Task.CompletedTask);

            var controller = new SearchController(apiServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.PerformSearch();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<SearchResultPayload>(okResult.Value);
            Assert.NotNull(payload);
            Assert.Equal("Sample text for searching", payload.Text);

            var sampleResult = payload.Results.FirstOrDefault(r => r.Subtext == "Sample");
            Assert.NotNull(sampleResult);
            Assert.Equal("1", sampleResult.Result);

            var textResult = payload.Results.FirstOrDefault(r => r.Subtext == "text");
            Assert.NotNull(textResult);
            Assert.Equal("8", textResult.Result);
        }

        [Fact]
        public async Task PerformSearch_NoMatches_ReturnsNoOutput()
        {
            // Arrange
            var apiServiceMock = new Mock<IApiService>();
            var loggerMock = new Mock<ILogger<SearchController>>();

            var textToSearch = new TextToSearch { text = "Sample text for searching" };
            var subTexts = new SubTexts { subTexts = new List<string> { "apple", "orange" } };

            apiServiceMock.Setup(x => x.GetTextToSearchAsync()).ReturnsAsync(textToSearch);
            apiServiceMock.Setup(x => x.GetSubTextsAsync()).ReturnsAsync(subTexts);
            apiServiceMock.Setup(x => x.PostResultsAsync(It.IsAny<SearchResultPayload>())).Returns(Task.CompletedTask);

            var controller = new SearchController(apiServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.PerformSearch();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<SearchResultPayload>(okResult.Value);
            Assert.NotNull(payload);

            var appleResult = payload.Results.FirstOrDefault(r => r.Subtext == "apple");
            Assert.NotNull(appleResult);
            Assert.Equal("<No Output>", appleResult.Result);

            var orangeResult = payload.Results.FirstOrDefault(r => r.Subtext == "orange");
            Assert.NotNull(orangeResult);
            Assert.Equal("<No Output>", orangeResult.Result);
        }

        [Fact]
        public async Task PerformSearch_ErrorFetchingText_ReturnsInternalServerError()
        {
            // Arrange
            var apiServiceMock = new Mock<IApiService>();
            var loggerMock = new Mock<ILogger<SearchController>>();


            var textToSearch = new TextToSearch { text = "Sample text for searching" };

            apiServiceMock.Setup(x => x.GetTextToSearchAsync()).ReturnsAsync(textToSearch);

            var controller = new SearchController(apiServiceMock.Object, loggerMock.Object);


            // Act
            var result = await controller.PerformSearch();

            // Assert
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);

        }

        [Fact]
        public async Task PerformSearch_ErrorFetchingSubTexts_ReturnsInternalServerError()
        {
            // Arrange
            var apiServiceMock = new Mock<IApiService>();
            var loggerMock = new Mock<ILogger<SearchController>>();

            apiServiceMock.Setup(x => x.GetTextToSearchAsync()).ReturnsAsync(new TextToSearch { text = "Sample text" });
            apiServiceMock.Setup(x => x.GetSubTextsAsync()).ThrowsAsync(new Exception("Error fetching subtexts"));

            var controller = new SearchController(apiServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.PerformSearch();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, objectResult.StatusCode);

        }
    }
}