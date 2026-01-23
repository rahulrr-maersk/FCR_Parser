using FcrParser.Models;

namespace FcrParser.Services;

public class DataExtractionOrchestrator
{
    private readonly ShipperExtractor _shipperExtractor;

    public DataExtractionOrchestrator(ShipperExtractor shipperExtractor)
    {
        _shipperExtractor = shipperExtractor;
    }

    public async Task<BookingData?> ExtractAllDataAsync(string csvFilePath, string fileName)
    {
        // Start both extractions in parallel for efficiency
        
        // Track 1: AI-based extraction (Shipper info from CLEANED text) - async/slow
        var shipperTask = ExtractShipperDataAsync(csvFilePath, fileName);
        
        // Track 2: Direct CSV column extraction (Marks & Numbers, Cargo Description from RAW CSV) - fast
        var columnTask = Task.Run(() => new
        {
            MarksAndNumbers = ColumnExtractor.ExtractMarksAndNumbers(csvFilePath),
            CargoDescription = ColumnExtractor.ExtractCargoDescription(csvFilePath)
        });

        // Wait for both to complete
        await Task.WhenAll(shipperTask, columnTask);

        // Get AI extraction result
        var bookingData = await shipperTask;
        if (bookingData == null)
        {
            return null;
        }

        // Append column data (extracted from RAW CSV) to AI result
        var columnData = await columnTask;
        bookingData.MarksAndNumbers = columnData.MarksAndNumbers;
        bookingData.CargoDescription = columnData.CargoDescription;

        return bookingData;
    }

    private async Task<BookingData?> ExtractShipperDataAsync(string csvFilePath, string fileName)
    {
        var cleanedText = await CleanCsvAsync(csvFilePath);
        return await _shipperExtractor.ExtractAsync(cleanedText, fileName);
    }

    private async Task<string> CleanCsvAsync(string csvFilePath)
    {
        var rawText = await File.ReadAllTextAsync(csvFilePath);
        var tempFile = Path.GetTempFileName();
        
        try
        {
            await File.WriteAllTextAsync(tempFile, rawText);
            return TextCleaner.Clean(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
