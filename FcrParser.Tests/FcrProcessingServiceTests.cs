using Xunit;
using Moq;
using FcrParser.Services;
using FcrParser.Services.AI;
using FcrParser.Models;
using System.IO;

namespace FcrParser.Tests;

public class FcrProcessingServiceTests
{
    private readonly string _testFolder;
    private readonly string _inputFolder;
    private readonly string _outputFolder;
    private readonly string _cleanedFolder;

    public FcrProcessingServiceTests()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), $"FcrTest_{Guid.NewGuid()}");
        _inputFolder = Path.Combine(_testFolder, "Input");
        _outputFolder = Path.Combine(_testFolder, "Output");
        _cleanedFolder = Path.Combine(_testFolder, "Cleaned");
        
        Directory.CreateDirectory(_inputFolder);
    }

    [Fact]
    public async Task ProcessAllFilesAsync_ShouldReturnZeroWhenNoFiles()
    {
        // Arrange
        var mockProvider = new Mock<IAIProvider>();
        var shipperExtractor = new ShipperExtractor(new[] { mockProvider.Object });
        var service = new FcrProcessingService(shipperExtractor, _testFolder);

        // Act
        var result = await service.ProcessAllFilesAsync();

        // Assert
        Assert.Equal(0, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldCreateOutputFiles()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,Shipper (complete name & address),x,x
x,x,x,x,x,x,x,Test Company,x,x
x,x,x,x,x,x,x,123 Test Street,x,x";
        var csvFile = CreateTestFile("test.csv", csvContent);

        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("TestProvider");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(@"{""ShipperName"": ""Test Company"", ""ShipperAddress"": ""123 Test Street""}");

        var shipperExtractor = new ShipperExtractor(new[] { mockProvider.Object });
        var service = new FcrProcessingService(shipperExtractor, _testFolder);

        // Act
        await service.ProcessSingleFileAsync(csvFile, "test.csv");

        // Assert
        Assert.True(File.Exists(Path.Combine(_outputFolder, "test.json")));
        Assert.True(File.Exists(Path.Combine(_outputFolder, "test.txt")));
        Assert.True(File.Exists(Path.Combine(_cleanedFolder, "test_cleaned.txt")));
    }

    [Fact]
    public async Task ProcessAllFilesAsync_ShouldCountSuccessesAndFailures()
    {
        // Arrange
        var csvContent = @"Shipper (complete name & address)
Test Company";
        CreateTestFile("file1.csv", csvContent);
        CreateTestFile("file2.csv", csvContent);

        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("TestProvider");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(@"{""ShipperName"": ""Test"", ""ShipperAddress"": ""Address""}");

        var shipperExtractor = new ShipperExtractor(new[] { mockProvider.Object });
        var service = new FcrProcessingService(shipperExtractor, _testFolder);

        // Act
        var result = await service.ProcessAllFilesAsync();

        // Assert
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailureCount);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldSaveCleanedText()
    {
        // Arrange
        var csvContent = @"Shipper (complete name & address)
My Test Company
My Test Address";
        var csvFile = CreateTestFile("clean_test.csv", csvContent);

        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("TestProvider");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(@"{""ShipperName"": ""My Test Company"", ""ShipperAddress"": ""My Test Address""}");

        var shipperExtractor = new ShipperExtractor(new[] { mockProvider.Object });
        var service = new FcrProcessingService(shipperExtractor, _testFolder);

        // Act
        await service.ProcessSingleFileAsync(csvFile, "clean_test.csv");

        // Assert
        var cleanedFilePath = Path.Combine(_cleanedFolder, "clean_test_cleaned.txt");
        Assert.True(File.Exists(cleanedFilePath));
        
        var cleanedContent = File.ReadAllText(cleanedFilePath);
        Assert.Contains("My Test Company", cleanedContent);
    }

    [Fact]
    public async Task ProcessSingleFileAsync_ShouldExtractMarksAndCargo()
    {
        // Arrange
        var csvContent = @"Marks & Numbers,Cargo Description
MARK001,Test Cargo
MARK002,More Cargo";
        var csvFile = CreateTestFile("columns_test.csv", csvContent);

        var mockProvider = new Mock<IAIProvider>();
        mockProvider.Setup(p => p.Name).Returns("TestProvider");
        mockProvider.Setup(p => p.GetResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(@"{""ShipperName"": ""Company"", ""ShipperAddress"": ""Address""}");

        var shipperExtractor = new ShipperExtractor(new[] { mockProvider.Object });
        var service = new FcrProcessingService(shipperExtractor, _testFolder);

        // Act
        await service.ProcessSingleFileAsync(csvFile, "columns_test.csv");

        // Assert
        var jsonPath = Path.Combine(_outputFolder, "columns_test.json");
        Assert.True(File.Exists(jsonPath));
        
        var jsonContent = File.ReadAllText(jsonPath);
        Assert.Contains("MARK001", jsonContent);
        Assert.Contains("Test Cargo", jsonContent);
    }

    private string CreateTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(_inputFolder, fileName);
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
