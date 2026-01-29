using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FcrParser.Services;
using FcrParser.Services.AI;
using System.Text.Json;
using FcrParse.Services;

// Build configuration for AI providers
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .Build();

// Setup dependency injection for AI providers
var services = new ServiceCollection();
services.AddHttpClient();
services.AddSingleton<IConfiguration>(configuration);
services.AddTransient<IAIProvider, CerebrasProvider>();
services.AddTransient<IAIProvider, GroqProvider>();
services.AddTransient<IAIProvider, DeepSeekProvider>();
services.AddTransient<IAIProvider, MistralProvider>();
services.AddTransient<IAIProvider, GeminiProvider>();
services.AddSingleton<ShipperExtractor>();
var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("=== FCR Parser - Simple & Reliable ===");
Console.WriteLine();

try
{
    // Use project root directory
    var projectRoot = Directory.GetCurrentDirectory();
    var inputFolder = Path.Combine(projectRoot, "Input");
    var outputFolder = Path.Combine(projectRoot, "Output");

    // Ensure folders exist
    Directory.CreateDirectory(outputFolder);

    // Find all CSV files in Input folder
    var csvFiles = Directory.GetFiles(inputFolder, "*.csv");

    if (csvFiles.Length == 0)
    {
        Console.WriteLine("⚠️  No CSV files found in Input folder.");
        return;
    }

    Console.WriteLine($"Found {csvFiles.Length} file(s) to process.\n");

    int successCount = 0;
    int failureCount = 0;

    foreach (var csvFile in csvFiles)
    {
        var fileName = Path.GetFileName(csvFile);
        Console.WriteLine($"Processing: {fileName}");


        try
        {
            // Run column extraction (fast, synchronous)
            var columnData = new Dictionary<string, List<string>>
            {
                ["MarksAndNumbers"] = ColumnExtractor.ExtractMarksAndNumbers(csvFile),
                ["CargoDescription"] = ColumnExtractor.ExtractCargoDescription(csvFile)
            };

            // Run AI extraction for shipper info in parallel (async, non-blocking)
            var shipperExtractor = serviceProvider.GetRequiredService<ShipperExtractor>();
            var shipperTask = ExtractShipperWithAI(csvFile, fileName, shipperExtractor);
            
            // Continue with column data while AI runs in background
            var extractedData = new Dictionary<string, object>
            {
                ["MarksAndNumbers"] = columnData["MarksAndNumbers"],
                ["CargoDescription"] = columnData["CargoDescription"],
                ["ShipperInfo"] = new Dictionary<string, string?>()
            };
            
            // Wait for AI extraction to complete (non-blocking)
            var shipperData = await shipperTask;
            if (shipperData != null)
            {
                extractedData["ShipperInfo"] = new Dictionary<string, string?>
                {
                    ["Name"] = shipperData.Value.ShipperName,
                    ["Address"] = shipperData.Value.ShipperAddress
                };
                Console.WriteLine("  ✓ Shipper info extracted via AI");
            }
            else
            {
                Console.WriteLine("  ⚠ Shipper info extraction failed (continuing...)");
            };

            // Save JSON output
            var outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".json";
            var outputPath = Path.Combine(outputFolder, outputFileName);

            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var jsonContent = JsonSerializer.Serialize(extractedData, jsonOptions);
            File.WriteAllText(outputPath, jsonContent);

            // Convert JSON to clean text format
            var txtFileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
            var txtOutputPath = Path.Combine(outputFolder, txtFileName);
            JsonToTextConverter.ConvertJsonToText(outputPath, txtOutputPath);

            Console.WriteLine($"✅ Completed: {fileName}");
            successCount++;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {fileName} - {ex.Message}");
            failureCount++;
        }

        Console.WriteLine();
    }

    // Summary
    Console.WriteLine("═══════════════════════════════");
    Console.WriteLine($"✅ Successful: {successCount}");
    if (failureCount > 0)
    {
        Console.WriteLine($"❌ Failed: {failureCount}");
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal Error: {ex.Message}");
}

async Task<(string? ShipperName, string? ShipperAddress)?> ExtractShipperWithAI(
    string csvFile, string fileName, ShipperExtractor extractor)
{
    try
    {
        // Clean CSV text for AI processing
        var rawText = await File.ReadAllTextAsync(csvFile);
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, rawText);
            var cleanedText = TextCleaner.Clean(tempFile);
            
            // Extract shipper data using AI
            var result = await extractor.ExtractAsync(cleanedText, fileName);
            if (result != null)
            {
                return (result.ShipperName, result.ShipperAddress);
            }
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  AI extraction error: {ex.Message}");
    }
    return null;
}

