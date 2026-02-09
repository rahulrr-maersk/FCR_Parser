using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FcrParser.Services;
using FcrParser.Services.AI;
using Polly;
using Polly.Extensions.Http;

// Build configuration for AI providers
// Priority: Environment Variables > User Secrets > appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables() // Allow GitHub Actions to inject secrets
    .Build();

// Polly retry policy: Handle transient HTTP errors with exponential backoff
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Handles 5xx and 408 errors
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Handle 429 rate limits
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // 2s, 4s, 8s

// Polly timeout policy: Prevent hanging on slow API responses
var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(30));

// Setup dependency injection
var services = new ServiceCollection();

// Configure HttpClient with Polly retry and timeout policies for resilience
services.AddHttpClient("AIProviderClient")
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(timeoutPolicy);

// Register default HttpClient factory
services.AddHttpClient();

// Register services
services.AddSingleton<IConfiguration>(configuration);
services.AddTransient<IAIProvider, CerebrasProvider>();
services.AddTransient<IAIProvider, GroqProvider>();
services.AddTransient<IAIProvider, DeepSeekProvider>();
services.AddTransient<IAIProvider, MistralProvider>();
services.AddTransient<IAIProvider, GeminiProvider>();
services.AddSingleton<ShipperExtractor>();
services.AddSingleton<FcrProcessingService>();

var serviceProvider = services.BuildServiceProvider();

// Run the application
Console.WriteLine("=== FCR Parser - Simple & Reliable ===");
Console.WriteLine();

try
{
    var processingService = serviceProvider.GetRequiredService<FcrProcessingService>();
    var result = await processingService.ProcessAllFilesAsync();

    // Summary
    Console.WriteLine("═══════════════════════════════");
    Console.WriteLine($"✅ Successful: {result.SuccessCount}");
    if (result.FailureCount > 0)
    {
        Console.WriteLine($"❌ Failed: {result.FailureCount}");
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal Error: {ex.Message}");
}
