namespace FcrParser.Models;

public class BookingData
{
    public string? FileName { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperAddress { get; set; }
    public MarksAndNumbers? MarksAndNumbers { get; set; }
}

public class MarksAndNumbers
{
    public int? TotalCartons { get; set; }
    public List<string>? BuyerItemCodes { get; set; }
    public string? PCINNumber { get; set; }
}
