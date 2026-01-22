namespace FcrParser.Models;

public class BookingData
{
    public string? FileName { get; set; }
    
    // Shipper Information
    public string? ShipperName { get; set; }
    public string? ShipperAddress { get; set; }
    
    // Consignee Information
    public string? ConsigneeName { get; set; }
    public string? ConsigneeAddress { get; set; }
    
    // Notifier Information
    public string? NotifierName { get; set; }
    public string? NotifierAddress { get; set; }
    
    // Cargo Information
    public string? CargoDescription { get; set; }
    public int? TotalCartons { get; set; }
    public int? TotalPieces { get; set; }
    
    // Weight and Volume
    public decimal? GrossWeightKgs { get; set; }
    public decimal? NetWeightKgs { get; set; }
    public decimal? VolumeCBM { get; set; }
    
    // Port Information
    public string? PortOfLoading { get; set; }
    public string? PortOfDischarge { get; set; }
    public string? ServiceType { get; set; }
    
    // Commercial Information
    public List<string>? PONumbers { get; set; }
    public List<string>? HSCodes { get; set; }
    public List<string>? ItemNumbers { get; set; }
    public List<string>? DPCICodes { get; set; }
    
    // Container Information
    public List<string>? ContainerNumbers { get; set; }
    public string? ContainerType { get; set; }
    
    // Additional Information
    public string? BookingReference { get; set; }
    public string? VesselName { get; set; }
    public string? VoyageNumber { get; set; }
    public DateTime? ShipmentDate { get; set; }
}
