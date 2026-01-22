using System.Text.Json;
using FcrParser.Models;
using FcrParser.Services.AI;

namespace FcrParser.Services;

public class ExtractionService
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly string _promptTemplate;

    public ExtractionService(IEnumerable<IAIProvider> providers)
    {
        _providers = providers;
        
        // Load the template from the output directory
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "ExtractionPrompt.txt");
        if (!File.Exists(path)) 
        {
            throw new FileNotFoundException($"Prompt template not found at: {path}. Did you set CopyToOutputDirectory?");
        }
        
        _promptTemplate = File.ReadAllText(path);
    }

    public async Task<BookingData?> ExtractAsync(string cleanText, string fileName)
    {
        // Inject the clean text into the prompt
        var prompt = _promptTemplate.Replace("{0}", cleanText);

        foreach (var provider in _providers)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"   Attempting extraction with {provider.Name}... ");
            Console.ResetColor();

            try 
            {
                var resultJson = await provider.GetResponseAsync(prompt);

                if (!string.IsNullOrWhiteSpace(resultJson))
                {
                    // Clean up potential markdown wrappers (```json ... ```)
                    resultJson = resultJson.Replace("```json", "").Replace("```", "").Trim();
                    
                    var data = JsonSerializer.Deserialize<BookingData>(resultJson, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    if (data != null)
                    {
                        data.FileName = fileName;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Success!");
                        Console.ResetColor();
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log failure but continue to the next provider
                Console.WriteLine($"Failed. ({ex.Message})");
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("   ❌ All providers failed.");
        Console.ResetColor();
        return null;
    }
}