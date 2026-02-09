using System.Text.Json;
using FcrParser.Models;
using FcrParse.Services;

namespace FcrParser.Services;

/// <summary>
/// Main service for processing FCR CSV files.
/// Handles file discovery, extraction, and output generation.
/// </summary>
public class FcrProcessingService
{
    private readonly ShipperExtractor _shipperExtractor;
    private readonly string _inputFolder;
    private readonly string _outputFolder;
    private readonly string _cleanedFolder;

    public FcrProcessingService(ShipperExtractor shipperExtractor, string? baseFolder = null)
    {
        _shipperExtractor = shipperExtractor;
        
        var projectRoot = baseFolder ?? Directory.GetCurrentDirectory();
        _inputFolder = Path.Combine(projectRoot, "Input");
        _outputFolder = Path.Combine(projectRoot, "Output");
        _cleanedFolder = Path.Combine(projectRoot, "Cleaned");
        
        // Ensure folders exist
        Directory.CreateDirectory(_outputFolder);
        Directory.CreateDirectory(_cleanedFolder);
    }

    /// <summary>
    /// Processes all CSV files in the Input folder
    /// </summary>
    public async Task<ProcessingResult> ProcessAllFilesAsync()
    {
        var result = new ProcessingResult();
        
        // Convert any XLSX files to CSV first
        ConvertExcelFilesToCsv();
        
        var csvFiles = Directory.GetFiles(_inputFolder, "*.csv");
        
        if (csvFiles.Length == 0)
        {
            Console.WriteLine("No CSV or Excel files found in Input folder.");
            return result;
        }

        Console.WriteLine($"Found {csvFiles.Length} file(s) to process.\n");

        foreach (var csvFile in csvFiles)
        {
            var fileName = Path.GetFileName(csvFile);
            Console.WriteLine($"Processing: {fileName}");

            try
            {
                await ProcessSingleFileAsync(csvFile, fileName);
                Console.WriteLine($"✅ Completed: {fileName}");
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {fileName} - {ex.Message}");
                result.FailureCount++;
            }

            Console.WriteLine();
        }

        return result;
    }
    
    /// <summary>
    /// Converts any XLSX files in the input folder to CSV
    /// </summary>
    private void ConvertExcelFilesToCsv()
    {
        var xlsxFiles = Directory.GetFiles(_inputFolder, "*.xlsx");
        
        if (xlsxFiles.Length == 0)
            return;
            
        Console.WriteLine($"Converting {xlsxFiles.Length} Excel file(s) to CSV...\n");
        
        foreach (var xlsxFile in xlsxFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(xlsxFile);
                var csvPath = Path.Combine(_inputFolder, $"{fileName}.csv");
                
                ExcelConverter.ConvertToCsv(xlsxFile, csvPath);
                Console.WriteLine($"  Converted: {Path.GetFileName(xlsxFile)} → {Path.GetFileName(csvPath)}");
                
                // Delete the original XLSX file
                File.Delete(xlsxFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Failed to convert {Path.GetFileName(xlsxFile)}: {ex.Message}");
            }
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Processes a single CSV file
    /// </summary>
    public async Task ProcessSingleFileAsync(string csvFilePath, string fileName)
    {
        // Step 1: Extract columns locally (fast)
        var marksAndNumbers = ColumnExtractor.ExtractMarksAndNumbers(csvFilePath);
        var cargoDescription = ColumnExtractor.ExtractCargoDescription(csvFilePath);

        // Step 2: Clean CSV and extract shipper info via AI
        var cleanedText = TextCleaner.Clean(csvFilePath);
        await SaveCleanedTextAsync(cleanedText, fileName);
        
        var shipperData = await _shipperExtractor.ExtractAsync(cleanedText, fileName);
        
        if (shipperData != null)
        {
            Console.WriteLine("  ✓ Shipper info extracted via AI");
        }
        else
        {
            Console.WriteLine("  ⚠ Shipper info extraction failed (continuing...)");
        }

        // Step 3: Build output data
        var extractedData = new Dictionary<string, object>
        {
            ["ShipperInfo"] = new Dictionary<string, string?>
            {
                ["Name"] = shipperData?.ShipperName,
                ["Address"] = shipperData?.ShipperAddress
            },
            ["MarksAndNumbers"] = marksAndNumbers,
            ["CargoDescription"] = cargoDescription,
        };

        // Step 4: Save JSON output
        var outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".json";
        var outputPath = Path.Combine(_outputFolder, outputFileName);
        
        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        var jsonContent = JsonSerializer.Serialize(extractedData, jsonOptions);
        await File.WriteAllTextAsync(outputPath, jsonContent);

        // Step 5: Convert JSON to text format
        var txtFileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
        var txtOutputPath = Path.Combine(_outputFolder, txtFileName);
        JsonToTextConverter.ConvertJsonToText(outputPath, txtOutputPath);
    }

    private async Task SaveCleanedTextAsync(string cleanedText, string fileName)
    {
        try
        {
            var cleanedFileName = Path.GetFileNameWithoutExtension(fileName) + "_cleaned.txt";
            var cleanedFilePath = Path.Combine(_cleanedFolder, cleanedFileName);
            
            await File.WriteAllTextAsync(cleanedFilePath, cleanedText);
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   📄 Saved cleaned data to: Cleaned\\{cleanedFileName}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"   ⚠️  Could not save cleaned file: {ex.Message}");
            Console.ResetColor();
        }
    }
}

/// <summary>
/// Result of processing multiple files
/// </summary>
public class ProcessingResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int TotalCount => SuccessCount + FailureCount;
}
