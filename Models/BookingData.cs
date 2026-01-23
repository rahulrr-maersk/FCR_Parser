namespace FcrParser.Models;

public class BookingData
{
    public string? FileName { get; set; }
    public string? ShipperName { get; set; }
    public string? ShipperAddress { get; set; }
    public List<string>? MarksAndNumbers { get; set; }
    public List<string>? CargoDescription { get; set; }
}
