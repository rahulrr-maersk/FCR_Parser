using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FcrParser.Services;
using FcrParser.Services.AI;
using FcrParser.Models;
using System.Text.Json;

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
services.AddSingleton<ExtractionService>();

var serviceProvider = services.BuildServiceProvider();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("     FCR Parser - AI-Powered Data Extraction Tool     ");
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.ResetColor();
Console.WriteLine();

try
{
    var extractionService = serviceProvider.GetRequiredService<ExtractionService>();

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
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("⚠️  No CSV files found in Input folder.");
        Console.ResetColor();
        return;
    }

    Console.WriteLine($"Found {csvFiles.Length} CSV file(s) to process.");
    Console.WriteLine();

    int successCount = 0;
    int failureCount = 0;

    foreach (var csvFile in csvFiles)
    {
        var fileName = Path.GetFileName(csvFile);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"📄 Processing: {fileName}");
        Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.ResetColor();

        try
        {
            // Step 1: Read CSV file
            Console.Write("   [1/5] Reading CSV file... ");
            var rawText = File.ReadAllText(csvFile);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // Step 2: Clean text using TextCleaner
            Console.Write("   [2/5] Cleaning text... ");
            
            // Save raw CSV temporarily for cleaning
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, rawText);
            var cleanedText = TextCleaner.Clean(tempFile);
            File.Delete(tempFile);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // Step 3: Save cleaned text to Cleaned folder
            Console.Write("   [3/5] Saving cleaned text... ");
            var cleanedFileName = Path.GetFileNameWithoutExtension(fileName) + "_cleaned.txt";
            var cleanedFilePath = Path.Combine(cleanedFolder, cleanedFileName);
            File.WriteAllText(cleanedFilePath, cleanedText);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();

            // Step 4: Extract data using AI
            Console.WriteLine("   [3/4] Extracting data with AI providers:");
            var extractedData = await extractionService.ExtractAsync(cleanedText, fileName);

            if (extractedData == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ❌ Failed to extract data from {fileName}");
                Console.ResetColor();
                failureCount++;
                Console.WriteLine();
                continue;
            }

            // Step 5: Validate and save JSON
            Console.Write("   [5/5] Validating and saving JSON... ");
            
            // Validate: Check if essential fields are present
            bool isValid = !string.IsNullOrWhiteSpace(extractedData.ShipperName) ||
                          !string.IsNullOrWhiteSpace(extractedData.ConsigneeName) ||
                          !string.IsNullOrWhiteSpace(extractedData.CargoDescription);

            if (!isValid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ (Validation failed: Missing essential data)");
                Console.ResetColor();
                failureCount++;
                Console.WriteLine();
                continue;
            }

            // Save to Output folder with original filename + .json
            var outputFileName = Path.GetFileNameWithoutExtension(fileName) + ".json";
            var outputPath = Path.Combine(outputFolder, outputFileName);

            var jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var jsonContent = JsonSerializer.Serialize(extractedData, jsonOptions);
            File.WriteAllText(outputPath, jsonContent);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"   ✅ Successfully saved to: {outputFileName}");
            Console.ResetColor();
            successCount++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"   ❌ Error processing {fileName}: {ex.Message}");
            Console.ResetColor();
            failureCount++;
        }

        Console.WriteLine();
    }

    // Summary
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.WriteLine("                    Processing Complete                ");
    Console.WriteLine("═══════════════════════════════════════════════════════");
    Console.ResetColor();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"✅ Successful: {successCount}");
    Console.ResetColor();
    
    if (failureCount > 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ Failed: {failureCount}");
        Console.ResetColor();
    }
    
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Fatal Error: {ex.Message}");
    Console.ResetColor();
}

