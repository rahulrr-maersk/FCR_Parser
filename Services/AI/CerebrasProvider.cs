using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FcrParser.Services.AI;

public class CerebrasProvider : IAIProvider
{
    public string Name => "Cerebras (Llama 3.1)";
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public CerebrasProvider(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["AI:CerebrasKey"] ?? "";
    }

    public async Task<string?> GetResponseAsync(string prompt)
    {
        if (string.IsNullOrEmpty(_apiKey)) return null;

        var requestBody = new
        {
            model = "llama3.1-8b",
            messages = new[] { 
                new { role = "user", content = prompt } 
            },
            temperature = 0.3,
            max_tokens = 2000
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.cerebras.ai/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        try
        {
            var res = await _http.SendAsync(request);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
        catch
        {
            return null;
        }
    }
}