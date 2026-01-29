using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FcrParser.Services;
using FcrParser.Services.AI;
using FcrParser.Models;
using System.Text.Json;
using FcrParse.Services;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>()
    .Build();

// Setup dependency injection
var services = new ServiceCollection();

// Register HttpClient for AI providers
services.AddHttpClient();

// Register AI providers in the specified order: Cerebras -> Groq -> Deepseek -> Mistral -> Gemini
services.AddSingleton<IConfiguration>(configuration);
services.AddTransient<IAIProvider, CerebrasProvider>();
services.AddTransient<IAIProvider, GroqProvider>();
services.AddTransient<IAIProvider, DeepSeekProvider>();
services.AddTransient<IAIProvider, MistralProvider>();
services.AddTransient<IAIProvider, GeminiProvider>();

// Register services
services.AddSingleton<ShipperExtractor>();
services.AddSingleton<DataExtractionOrchestrator>();

var serviceProvider = services.BuildServiceProvider();

Console.WriteLine("FCR Parser - AI-Powered Data Extraction");
Console.WriteLine();

try
{
    var orchestrator = serviceProvider.GetRequiredService<DataExtractionOrchestrator>();

    // Use project root directory instead of build output directory for easier file access
    var projectRoot = Directory.GetCurrentDirectory();
    var inputFolder = Path.Combine(projectRoot, "Input");
    var outputFolder = Path.Combine(projectRoot, "Output");
    var cleanedFolder = Path.Combine(projectRoot, "Cleaned");

    // Ensure folders exist
    Directory.CreateDirectory(outputFolder);
    Directory.CreateDirectory(cleanedFolder);

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
            // Use orchestrator to extract all data (shipper + columns)
            var extractedData = await orchestrator.ExtractAllDataAsync(csvFile, fileName);

            if (extractedData == null)
            {
                Console.WriteLine($"❌ Failed: {fileName}");
                failureCount++;
                continue;
            }

            // Save cleaned text for reference
            var cleanedFileName = Path.GetFileNameWithoutExtension(fileName) + "_cleaned.txt";
            var cleanedFilePath = Path.Combine(cleanedFolder, cleanedFileName);
            var rawText = File.ReadAllText(csvFile);
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, rawText);
            var cleanedText = TextCleaner.Clean(tempFile);
            File.Delete(tempFile);
            File.WriteAllText(cleanedFilePath, cleanedText);

            // Save JSON output
            var outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".json";
            var outputPath = Path.Combine(outputFolder, outputFileName);

            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var jsonContent = JsonSerializer.Serialize(extractedData, jsonOptions);
            File.WriteAllText(outputPath, jsonContent);

            // Convert JSON to clean text format
            var txtFileName = Path.GetFileNameWithoutExtension(fileName) + ".txt";
            var txtOutputPath = Path.Combine(outputFolder, txtFileName);
            JsonToTextConverter.ConvertJsonToText(outputPath, txtOutputPath);

            Console.WriteLine($"✅ Success: {outputFileName}");
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

