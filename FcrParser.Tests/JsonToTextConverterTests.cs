using Xunit;
using FcrParse.Services;
using System.IO;
using System.Text.Json;

namespace FcrParser.Tests;

public class JsonToTextConverterTests
{
    [Fact]
    public void ConvertJsonToText_ShouldConvertSimpleJson()
    {
        // Arrange
        var jsonData = new
        {
            MarksAndNumbers = new[] { "MARK1", "MARK2" },
            CargoDescription = new[] { "CARGO1", "CARGO2" }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Marks And Numbers:", result);
            Assert.Contains("MARK1", result);
            Assert.Contains("MARK2", result);
            Assert.Contains("Cargo Description:", result);
            Assert.Contains("CARGO1", result);
            Assert.Contains("CARGO2", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleNestedObjects()
    {
        // Arrange
        var jsonData = new
        {
            ShipperInfo = new
            {
                Name = "Acme Corp",
                Address = "123 Main St, City, Country"
            }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Shipper Info:", result);
            Assert.Contains("Name: Acme Corp", result);
            Assert.Contains("Address: 123 Main St, City, Country", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldFormatPropertyNames()
    {
        // Arrange
        var jsonData = new
        {
            MarksAndNumbers = new[] { "TEST" },
            CargoDescription = new[] { "CARGO" },
            ShipperInfo = new { Name = "Test" }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            // Should convert camelCase to "Readable Format"
            Assert.Contains("Marks And Numbers:", result);
            Assert.Contains("Cargo Description:", result);
            Assert.Contains("Shipper Info:", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleEmptyArrays()
    {
        // Arrange
        var jsonData = new
        {
            MarksAndNumbers = new string[] { },
            CargoDescription = new string[] { }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Marks And Numbers:", result);
            Assert.Contains("Cargo Description:", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleNullValues()
    {
        // Arrange
        var jsonData = new
        {
            ShipperInfo = new
            {
                Name = (string?)null,
                Address = (string?)null
            }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Shipper Info:", result);
            Assert.Contains("Name: N/A", result);
            Assert.Contains("Address: N/A", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldAddBlankLinesBetweenSections()
    {
        // Arrange
        var jsonData = new
        {
            MarksAndNumbers = new[] { "MARK1" },
            CargoDescription = new[] { "CARGO1" }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            var lines = result.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            
            // Should have blank lines between sections
            var blankLineCount = lines.Count(l => string.IsNullOrWhiteSpace(l));
            Assert.True(blankLineCount >= 2);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleComplexRealWorldData()
    {
        // Arrange
        var jsonData = new
        {
            MarksAndNumbers = new[] 
            { 
                "MSKU1234567",
                "S/O: 123456789",
                "---",
                "MSKU7654321"
            },
            CargoDescription = new[] 
            { 
                "ELECTRONIC COMPONENTS",
                "HS CODE: 8542.31",
                "MADE IN CHINA"
            },
            ShipperInfo = new
            {
                Name = "ABC Electronics Ltd.",
                Address = "ABC Electronics Ltd., 123 Industrial Zone, Shenzhen, Guangdong - 518000, China"
            }
        };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("MSKU1234567", result);
            Assert.Contains("S/O: 123456789", result);
            Assert.Contains("ELECTRONIC COMPONENTS", result);
            Assert.Contains("HS CODE: 8542.31", result);
            Assert.Contains("ABC Electronics Ltd.", result);
            Assert.Contains("Shenzhen", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldCreateOutputFile()
    {
        // Arrange
        var jsonData = new { TestProperty = new[] { "Value" } };
        var (jsonFile, txtFile) = CreateTempFiles(jsonData);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            Assert.True(File.Exists(txtFile));
            Assert.True(new FileInfo(txtFile).Length > 0);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleArrayWithNumbers()
    {
        // Arrange
        var jsonContent = @"{
            ""Numbers"": [123, 456, 789],
            ""MixedArray"": [""text"", 123, true, null]
        }";
        var jsonFile = Path.GetTempFileName();
        var txtFile = Path.GetTempFileName();
        File.WriteAllText(jsonFile, jsonContent);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Numbers:", result);
            Assert.Contains("123", result);
            Assert.Contains("456", result);
            Assert.Contains("789", result);
            Assert.Contains("Mixed Array:", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleArrayWithObjects()
    {
        // Arrange
        var jsonContent = @"{
            ""Items"": [
                {""Name"": ""Item1"", ""Value"": 100},
                {""Name"": ""Item2"", ""Value"": 200}
            ]
        }";
        var jsonFile = Path.GetTempFileName();
        var txtFile = Path.GetTempFileName();
        File.WriteAllText(jsonFile, jsonContent);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Items:", result);
            Assert.Contains("Item1", result);
            Assert.Contains("Item2", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleNumberProperty()
    {
        // Arrange
        var jsonContent = @"{
            ""Count"": 42,
            ""Price"": 99.99,
            ""Active"": true
        }";
        var jsonFile = Path.GetTempFileName();
        var txtFile = Path.GetTempFileName();
        File.WriteAllText(jsonFile, jsonContent);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Count:", result);
            Assert.Contains("42", result);
            Assert.Contains("Price:", result);
            Assert.Contains("99.99", result);
            Assert.Contains("Active:", result);
            Assert.Contains("True", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    [Fact]
    public void ConvertJsonToText_ShouldHandleSimpleStringProperty()
    {
        // Arrange
        var jsonContent = @"{
            ""Title"": ""Simple String Value"",
            ""Description"": ""Another string"",
            ""Note"": ""Test Note""
        }";
        var jsonFile = Path.GetTempFileName();
        var txtFile = Path.GetTempFileName();
        File.WriteAllText(jsonFile, jsonContent);

        try
        {
            // Act
            JsonToTextConverter.ConvertJsonToText(jsonFile, txtFile);

            // Assert
            var result = File.ReadAllText(txtFile);
            Assert.Contains("Title:", result);
            Assert.Contains("Simple String Value", result);
            Assert.Contains("Description:", result);
            Assert.Contains("Another string", result);
            Assert.Contains("Note:", result);
            Assert.Contains("Test Note", result);
        }
        finally
        {
            CleanupFiles(jsonFile, txtFile);
        }
    }

    private (string jsonFile, string txtFile) CreateTempFiles(object data)
    {
        var jsonFile = Path.GetTempFileName();
        var txtFile = Path.GetTempFileName();
        
        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        var jsonContent = JsonSerializer.Serialize(data, jsonOptions);
        File.WriteAllText(jsonFile, jsonContent);
        
        return (jsonFile, txtFile);
    }

    private void CleanupFiles(params string[] files)
    {
        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
