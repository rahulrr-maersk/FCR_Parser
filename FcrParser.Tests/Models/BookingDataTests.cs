using Xunit;
using FcrParser.Models;

namespace FcrParser.Tests.Models;

public class BookingDataTests
{
    [Fact]
    public void BookingData_ShouldInitializeWithDefaultValues()
    {
        // Act
        var bookingData = new BookingData();

        // Assert
        Assert.Null(bookingData.FileName);
        Assert.Null(bookingData.ShipperName);
        Assert.Null(bookingData.ShipperAddress);
        Assert.Null(bookingData.MarksAndNumbers);
        Assert.Null(bookingData.CargoDescription);
    }

    [Fact]
    public void BookingData_ShouldAllowSettingFileName()
    {
        // Arrange
        var bookingData = new BookingData();

        // Act
        bookingData.FileName = "test.csv";

        // Assert
        Assert.Equal("test.csv", bookingData.FileName);
    }

    [Fact]
    public void BookingData_ShouldAllowSettingShipperName()
    {
        // Arrange
        var bookingData = new BookingData();

        // Act
        bookingData.ShipperName = "Acme Corp";

        // Assert
        Assert.Equal("Acme Corp", bookingData.ShipperName);
    }

    [Fact]
    public void BookingData_ShouldAllowSettingShipperAddress()
    {
        // Arrange
        var bookingData = new BookingData();

        // Act
        bookingData.ShipperAddress = "123 Main St, City, Country";

        // Assert
        Assert.Equal("123 Main St, City, Country", bookingData.ShipperAddress);
    }

    [Fact]
    public void BookingData_ShouldAllowSettingMarksAndNumbers()
    {
        // Arrange
        var bookingData = new BookingData();
        var marks = new List<string> { "MARK1", "MARK2" };

        // Act
        bookingData.MarksAndNumbers = marks;

        // Assert
        Assert.Equal(marks, bookingData.MarksAndNumbers);
        Assert.Equal(2, bookingData.MarksAndNumbers.Count);
    }

    [Fact]
    public void BookingData_ShouldAllowSettingCargoDescription()
    {
        // Arrange
        var bookingData = new BookingData();
        var cargo = new List<string> { "ELECTRONICS", "FRAGILE" };

        // Act
        bookingData.CargoDescription = cargo;

        // Assert
        Assert.Equal(cargo, bookingData.CargoDescription);
        Assert.Equal(2, bookingData.CargoDescription.Count);
    }

    [Fact]
    public void BookingData_ShouldAllowNullValues()
    {
        // Arrange
        var bookingData = new BookingData
        {
            FileName = null,
            ShipperName = null,
            ShipperAddress = null,
            MarksAndNumbers = null,
            CargoDescription = null
        };

        // Assert
        Assert.Null(bookingData.FileName);
        Assert.Null(bookingData.ShipperName);
        Assert.Null(bookingData.ShipperAddress);
        Assert.Null(bookingData.MarksAndNumbers);
        Assert.Null(bookingData.CargoDescription);
    }

    [Fact]
    public void BookingData_ShouldAllowCompleteDataSet()
    {
        // Arrange & Act
        var bookingData = new BookingData
        {
            FileName = "FCR_12345.csv",
            ShipperName = "ABC Electronics Ltd.",
            ShipperAddress = "123 Industrial Zone, Shenzhen, China",
            MarksAndNumbers = new List<string> { "MSKU1234567", "S/O: 123456789" },
            CargoDescription = new List<string> { "ELECTRONIC COMPONENTS", "HS CODE: 8542.31" }
        };

        // Assert
        Assert.Equal("FCR_12345.csv", bookingData.FileName);
        Assert.Equal("ABC Electronics Ltd.", bookingData.ShipperName);
        Assert.Equal("123 Industrial Zone, Shenzhen, China", bookingData.ShipperAddress);
        Assert.Equal(2, bookingData.MarksAndNumbers.Count);
        Assert.Equal(2, bookingData.CargoDescription.Count);
    }

    [Fact]
    public void BookingData_ShouldAllowEmptyLists()
    {
        // Arrange & Act
        var bookingData = new BookingData
        {
            MarksAndNumbers = new List<string>(),
            CargoDescription = new List<string>()
        };

        // Assert
        Assert.NotNull(bookingData.MarksAndNumbers);
        Assert.Empty(bookingData.MarksAndNumbers);
        Assert.NotNull(bookingData.CargoDescription);
        Assert.Empty(bookingData.CargoDescription);
    }
}
