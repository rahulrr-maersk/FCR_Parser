using Xunit;
using Moq;
using FcrParser.Services;
using FcrParser.Services.AI;
using FcrParser.Models;
using System.Text.Json;

namespace FcrParser.Tests;

public class ShipperExtractorTests
{
    [Fact]
    public async Task ExtractAsync_ShouldReturnDataFromFirstSuccessfulProvider()
    {
        // Arrange
        var mockProvider1 = new Mock<IAIProvider>();
        mockProvider1.Setup(p => p.Name).Returns("Provider1");
        mockProvider1.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var mockProvider2 = new Mock<IAIProvider>();
        mockProvider2.Setup(p => p.Name).Returns("Provider2");
        var validJson = JsonSerializer.Serialize(new
        {
            ShipperName = "Acme Corp",
            ShipperAddress = "123 Main St"
        });
        mockProvider2.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(validJson);

        var providers = new[] { mockProvider1.Object, mockProvider2.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.ShipperName);
        Assert.Equal("123 Main St", result.ShipperAddress);
        Assert.Equal("test.csv", result.FileName);
    }

    [Fact]
    public async Task ExtractAsync_ShouldReturnNullWhenAllProvidersFail()
    {
        // Arrange
        var mockProvider1 = new Mock<IAIProvider>();
        mockProvider1.Setup(p => p.Name).Returns("Provider1");
        mockProvider1.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var mockProvider2 = new Mock<IAIProvider>();
        mockProvider2.Setup(p => p.Name).Returns("Provider2");
        mockProvider2.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("API Error"));

        var providers = new[] { mockProvider1.Object, mockProvider2.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExtractAsync_ShouldHandleMarkdownWrappedJson()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        var jsonWithMarkdown = @"```json
{
  ""ShipperName"": ""Test Company"",
  ""ShipperAddress"": ""456 Test Ave""
}
```";
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(jsonWithMarkdown);

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Company", result.ShipperName);
        Assert.Equal("456 Test Ave", result.ShipperAddress);
    }

    [Fact]
    public async Task ExtractAsync_ShouldHandleNullShipperFields()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        var jsonWithNulls = JsonSerializer.Serialize(new
        {
            ShipperName = (string?)null,
            ShipperAddress = (string?)null
        });
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(jsonWithNulls);

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ShipperName);
        Assert.Null(result.ShipperAddress);
    }

    [Fact]
    public async Task ExtractAsync_ShouldSetFileName()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        var validJson = JsonSerializer.Serialize(new
        {
            ShipperName = "Company",
            ShipperAddress = "Address"
        });
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(validJson);

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "FCR_12345.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FCR_12345.csv", result.FileName);
    }

    [Fact]
    public async Task ExtractAsync_ShouldHandleInvalidJson()
    {
        // Arrange
        var mockProvider1 = new Mock<IAIProvider>();
        mockProvider1.Setup(p => p.Name).Returns("Provider1");
        mockProvider1.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync("This is not valid JSON");

        var mockProvider2 = new Mock<IAIProvider>();
        mockProvider2.Setup(p => p.Name).Returns("Provider2");
        var validJson = JsonSerializer.Serialize(new
        {
            ShipperName = "Fallback Company",
            ShipperAddress = "Fallback Address"
        });
        mockProvider2.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(validJson);

        var providers = new[] { mockProvider1.Object, mockProvider2.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fallback Company", result.ShipperName);
    }

    [Fact]
    public async Task ExtractAsync_ShouldHandleEmptyResponse()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync("");

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExtractAsync_ShouldHandleWhitespaceResponse()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync("   \n\t  ");

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExtractAsync_ShouldUseCaseInsensitiveDeserialization()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("Provider1");
        // JSON with different casing
        var jsonDifferentCase = @"{
            ""shippername"": ""Test Corp"",
            ""shipperaddress"": ""789 Test Blvd""
        }";
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(jsonDifferentCase);

        var providers = new[] { mockProvider.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        var result = await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Corp", result.ShipperName);
        Assert.Equal("789 Test Blvd", result.ShipperAddress);
    }

    [Fact]
    public async Task ExtractAsync_ShouldTryAllProvidersInOrder()
    {
        // Arrange
        var callOrder = new List<string>();

        var mockProvider1 = new Mock<IAIProvider>();
        mockProvider1.Setup(p => p.Name).Returns("Provider1");
        mockProvider1.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .Callback(() => callOrder.Add("Provider1"))
            .ReturnsAsync((string?)null);

        var mockProvider2 = new Mock<IAIProvider>();
        mockProvider2.Setup(p => p.Name).Returns("Provider2");
        mockProvider2.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .Callback(() => callOrder.Add("Provider2"))
            .ReturnsAsync((string?)null);

        var mockProvider3 = new Mock<IAIProvider>();
        mockProvider3.Setup(p => p.Name).Returns("Provider3");
        mockProvider3.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .Callback(() => callOrder.Add("Provider3"))
            .ReturnsAsync(JsonSerializer.Serialize(new { ShipperName = "Test", ShipperAddress = "Test" }));

        var providers = new[] { mockProvider1.Object, mockProvider2.Object, mockProvider3.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.Equal(new[] { "Provider1", "Provider2", "Provider3" }, callOrder);
    }

    [Fact]
    public async Task ExtractAsync_ShouldStopAtFirstSuccess()
    {
        // Arrange
        var callOrder = new List<string>();

        var mockProvider1 = new Mock<IAIProvider>();
        mockProvider1.Setup(p => p.Name).Returns("Provider1");
        mockProvider1.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .Callback(() => callOrder.Add("Provider1"))
            .ReturnsAsync(JsonSerializer.Serialize(new { ShipperName = "Success", ShipperAddress = "Address" }));

        var mockProvider2 = new Mock<IAIProvider>();
        mockProvider2.Setup(p => p.Name).Returns("Provider2");
        mockProvider2.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .Callback(() => callOrder.Add("Provider2"))
            .ReturnsAsync((string?)null);

        var providers = new[] { mockProvider1.Object, mockProvider2.Object };
        var extractor = new ShipperExtractor(providers);

        // Act
        await extractor.ExtractAsync("test data", "test.csv");

        // Assert
        Assert.Single(callOrder);
        Assert.Equal("Provider1", callOrder[0]);
    }
}
