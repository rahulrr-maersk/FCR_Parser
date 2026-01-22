using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace FcrParser.Services.AI;

public class GeminiProvider : IAIProvider
{
    public string Name => "Gemini";
    private readonly HttpClient _http;
    private readonly string _apiKey;

    private readonly string[] _models = { 
        "gemini-2.0-flash-exp",
        "gemini-1.5-flash",
        "gemini-1.5-pro"
    };

    public GeminiProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["AI:GeminiKey"] ?? "";
    }

    public async Task<string?> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) return null;

        var requestBody = new 
        { 
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new { temperature = 0.3, maxOutputTokens = 2000 }
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        foreach (var model in _models)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_apiKey}";
            
            try
            {
                var res = await _http.PostAsync(url, jsonContent);
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    var obj = JsonSerializer.Deserialize<GeminiResponse>(json);
                    return obj?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                }
            }
            catch { /* Try next model */ }
        }

        return null;
    }

    private class GeminiResponse { [JsonPropertyName("candidates")] public List<Candidate>? Candidates { get; set; } }
    private class Candidate { [JsonPropertyName("content")] public Content? Content { get; set; } }
    private class Content { [JsonPropertyName("parts")] public List<Part>? Parts { get; set; } }
    private class Part { [JsonPropertyName("text")] public string? Text { get; set; } }
}