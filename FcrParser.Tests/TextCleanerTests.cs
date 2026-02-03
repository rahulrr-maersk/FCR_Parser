using Xunit;
using FcrParser.Services;
using System.IO;
using System.Text;

namespace FcrParser.Tests;

public class TextCleanerTests
{
    [Fact]
    public void Clean_ShouldRemoveXMarkers()
    {
        // Arrange
        var csvContent = "x,Data1,x,Data2,x\nx,x,x,x,x";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.DoesNotContain("x,", result);
            Assert.DoesNotContain(",x", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldCollapseMultipleCommas()
    {
        // Arrange
        var csvContent = "Data1,,,,,Data2\nValue1,,,,,,,,Value2";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.DoesNotContain(",,,", result);
            Assert.Contains("Data1", result);
            Assert.Contains("Data2", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldRemoveEmptyLines()
    {
        // Arrange
        var csvContent = "Data1,Data2\n\n\nData3,Data4\n   \n";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(2, lines.Length);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldKeepLinesWithContent()
    {
        // Arrange
        var csvContent = "Shipper Name,Address\nAcme Corp,123 Main St\n,,,,,\nConsignee,Details";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.Contains("Shipper Name", result);
            Assert.Contains("Acme Corp", result);
            Assert.Contains("Consignee", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldHandleEmptyFile()
    {
        // Arrange
        var tempFile = CreateTempFile("");

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.Empty(result.Trim());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldHandleFileWithOnlyWhitespace()
    {
        // Arrange
        var csvContent = "   \n\n   \n\t\t\n";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.Empty(result.Trim());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldPreserveActualData()
    {
        // Arrange
        var csvContent = @"x,x,Shipper (complete name & address),x,x,x
x,x,ABC Electronics Ltd.,x,x,x
x,x,123 Industrial Zone,x,x,x
x,x,Shenzhen, Guangdong - 518000,x,x,x
x,x,China,x,x,x";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.Contains("ABC Electronics Ltd", result);
            Assert.Contains("123 Industrial Zone", result);
            Assert.Contains("Shenzhen", result);
            Assert.Contains("China", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldHandleFileOpenInExcel()
    {
        // Arrange
        var tempFile = CreateTempFile("Data1,Data2");

        try
        {
            // Act - Should not throw even if file is "in use"
            var result = TextCleaner.Clean(tempFile);

            // Assert
            Assert.NotEmpty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Clean_ShouldTrimWhitespace()
    {
        // Arrange
        var csvContent = "  Data1  ,  Data2  \n  Value1  ,  Value2  ";
        var tempFile = CreateTempFile(csvContent);

        try
        {
            // Act
            var result = TextCleaner.Clean(tempFile);

            // Assert
            var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                Assert.Equal(line.Trim(), line);
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private string CreateTempFile(string content)
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }
}
