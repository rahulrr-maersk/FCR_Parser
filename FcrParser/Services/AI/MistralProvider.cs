using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FcrParser.Services.AI;

public class MistralProvider : IAIProvider
{
    public string Name => "Mistral AI";
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public MistralProvider(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["AI:MistralKey"] ?? "";
    }

    public async Task<string?> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) return null;

        var requestBody = new
        {
            model = "mistral-small-latest",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", _apiKey);

        try
        {
            var response = await _httpClient.PostAsync("https://api.mistral.ai/v1/chat/completions", content);
            response.EnsureSuccessStatusCode();

            var resJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
        }
        catch
        {
            return null;
        }
    }
}