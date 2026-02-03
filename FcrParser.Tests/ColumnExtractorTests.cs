using Xunit;
using FcrParser.Services;
using System.IO;
using System.Linq;

namespace FcrParser.Tests;

public class ColumnExtractorTests
{
    [Fact]
    public void ExtractColumn_ShouldFindMarksAndNumbersHeader()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks & Numbers,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
1659,,,,,,,,,,,BEDDING,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
CARTONS,,,,,,,,,,,100%Cotton Bed Sheets,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("1659", result);
            Assert.Contains("CARTONS", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldFilterOutColumnHeaders()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks & Numbers,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
1659,,,,,,,,,,,BEDDING,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
Cargo Description,,,,,,,,,,,Test Data,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.DoesNotContain("Cargo Description", result);
            Assert.DoesNotContain("Marks & Numbers", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldReplaceMultipleSpacesWithSlash()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks & Numbers,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
""NET WEIGHT :-  11619.132 KGS"",,,,,,,,,,,BEDDING,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            var weightEntry = result.FirstOrDefault(r => r.Contains("NET WEIGHT"));
            Assert.NotNull(weightEntry);
            Assert.Contains(" / ", weightEntry);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldRemoveXMarkers()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks & Numbers,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
xxxTEST123xxx,,,,,,,,,,,BEDDING,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.Contains("TEST123", result);
            Assert.DoesNotContain(result, r => r.Contains("x"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldHandleEmptyFile()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, "");

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldHandleMissingHeader()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Some Other Header,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
1659,,,,,,,,,,,BEDDING,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.Empty(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractCargoDescription_ShouldExtractCorrectly()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks & Numbers,,,,,,,,,,,Cargo Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
1659,,,,,,,,,,,ELECTRONICS,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
CARTONS,,,,,,,,,,,HS CODE: 8542.31,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractCargoDescription(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("ELECTRONICS", result);
            Assert.Contains("HS CODE: 8542.31", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractCargoDescription_ShouldHandleAlternativeColumnNames()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks,,,,,,,,,,,Goods Description,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
TEST,,,,,,,,,,,MACHINERY,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractCargoDescription(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("MACHINERY", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractShipperInfo_ShouldExtractCorrectly()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks,,,,,,,,,,,Shipper,,,,,,,,,,,Cargo,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
TEST,,,,,,,,,,,ABC Corp,,,,,,,,,,,DATA,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
,,,,,,,,,,,123 Main St,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractShipperInfo(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("ABC Corp", result);
            Assert.Contains("123 Main St", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractConsigneeInfo_ShouldExtractCorrectly()
    {
        // Arrange
        var csvContent = @"x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,x,
Marks,,,,,,,,,,,Consignee,,,,,,,,,,,Cargo,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
TEST,,,,,,,,,,,ABC Ltd,,,,,,,,,,,DATA,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
,,,,,,,,,,,456 Oak Ave,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractConsigneeInfo(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("ABC Ltd", result);
            Assert.Contains("456 Oak Ave", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldHandleMultipleEmptyRows()
    {
        // Arrange
        var csvContent = @"Marks & Numbers,Cargo Description
DATA1,CARGO1
,
,
,
DATA2,CARGO2";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.Contains("DATA1", result);
            Assert.Contains("---", result);
            Assert.Contains("DATA2", result);
            // Should only have one separator for consecutive empty rows
            Assert.Equal(1, result.Count(r => r == "---"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldHandleSpannedColumns()
    {
        // Arrange
        var csvContent = @"Marks & Numbers,,,,,Cargo Description
DATA1,MORE1,MORE2,,DATA_CARGO,
DATA2,MORE3,,,DATA_CARGO2,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.Contains("DATA1", result);
            Assert.Contains("MORE1", result);
            Assert.Contains("MORE2", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldDecodeHtmlEntities()
    {
        // Arrange
        var csvContent = @"Marks &amp; Numbers,Cargo Description
TEST123,DATA";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("TEST123", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldRemoveTrailingSeparators()
    {
        // Arrange
        var csvContent = @"Marks & Numbers,Cargo Description
DATA1,CARGO1
,
,";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.DoesNotContain(result, r => r == "---" && result.IndexOf(r) == result.Count - 1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ExtractColumn_ShouldHandleCaseInsensitiveHeaders()
    {
        // Arrange
        var csvContent = @"marks and numbers,cargo description
TEST,DATA";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        try
        {
            // Act
            var result = ColumnExtractor.ExtractMarksAndNumbers(tempFile);

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("TEST", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
