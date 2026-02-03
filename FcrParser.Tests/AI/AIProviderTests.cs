using Xunit;
using Moq;
using Moq.Protected;
using FcrParser.Services.AI;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;

namespace FcrParser.Tests.AI;

public class CerebrasProviderTests
{
    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenApiKeyMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:CerebrasKey"]).Returns((string?)null);
        
        var httpClient = new HttpClient();
        var provider = new CerebrasProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnContentOnSuccess()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:CerebrasKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = "AI Response" }
                }
            }
        });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new CerebrasProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("AI Response", result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullOnHttpError()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:CerebrasKey"]).Returns("test-api-key");

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new CerebrasProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:CerebrasKey"]).Returns("test-key");
        var httpClient = new HttpClient();
        var provider = new CerebrasProvider(httpClient, mockConfig.Object);

        // Act
        var name = provider.Name;

        // Assert
        Assert.Equal("Cerebras (Llama 3.1)", name);
    }
}

public class GroqProviderTests
{
    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenApiKeyMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GroqKey"]).Returns((string?)null);
        
        var httpClient = new HttpClient();
        var provider = new GroqProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnContentOnSuccess()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GroqKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = "Groq AI Response" }
                }
            }
        });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new GroqProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("Groq AI Response", result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullOnHttpError()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GroqKey"]).Returns("test-api-key");

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new GroqProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GroqKey"]).Returns("test-key");
        var httpClient = new HttpClient();
        var provider = new GroqProvider(httpClient, mockConfig.Object);

        // Act
        var name = provider.Name;

        // Assert
        Assert.Equal("Groq (Llama 3.1)", name);
    }
}

public class DeepSeekProviderTests
{
    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenApiKeyMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DeepseekKey"]).Returns((string?)null);
        
        var httpClient = new HttpClient();
        var provider = new DeepSeekProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnContentOnSuccess()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DeepseekKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = "DeepSeek AI Response" }
                }
            }
        });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new DeepSeekProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("DeepSeek AI Response", result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullOnHttpError()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DeepseekKey"]).Returns("test-api-key");

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new DeepSeekProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DeepseekKey"]).Returns("test-key");
        var httpClient = new HttpClient();
        var provider = new DeepSeekProvider(httpClient, mockConfig.Object);

        // Act
        var name = provider.Name;

        // Assert
        Assert.Equal("DeepSeek", name);
    }
}

public class MistralProviderTests
{
    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenApiKeyMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:MistralKey"]).Returns((string?)null);
        
        var httpClient = new HttpClient();
        var provider = new MistralProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnContentOnSuccess()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:MistralKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new { content = "Mistral AI Response" }
                }
            }
        });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new MistralProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("Mistral AI Response", result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullOnHttpError()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:MistralKey"]).Returns("test-api-key");

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new MistralProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:MistralKey"]).Returns("test-key");
        var httpClient = new HttpClient();
        var provider = new MistralProvider(httpClient, mockConfig.Object);

        // Act
        var name = provider.Name;

        // Assert
        Assert.Equal("Mistral AI", name);
    }
}

public class GeminiProviderTests
{
    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenApiKeyMissing()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GeminiKey"]).Returns((string?)null);
        
        var httpClient = new HttpClient();
        var provider = new GeminiProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnContentOnSuccess()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GeminiKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new
                {
                    content = new 
                    { 
                        parts = new[] { new { text = "Gemini AI Response" } } 
                    }
                }
            }
        });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new GeminiProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("Gemini AI Response", result);
    }

    [Fact]
    public async Task GetResponseAsync_ShouldFallbackToNextModelOnFailure()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GeminiKey"]).Returns("test-api-key");

        var responseJson = JsonSerializer.Serialize(new
        {
            candidates = new[]
            {
                new
                {
                    content = new 
                    { 
                        parts = new[] { new { text = "Fallback Model Response" } } 
                    }
                }
            }
        });

        var callCount = 0;
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                // First model fails, second succeeds
                if (callCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseJson)
                };
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new GeminiProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Equal("Fallback Model Response", result);
        Assert.Equal(2, callCount); // Should have tried 2 models
    }

    [Fact]
    public async Task GetResponseAsync_ShouldReturnNullWhenAllModelsFail()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GeminiKey"]).Returns("test-api-key");

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var provider = new GeminiProvider(httpClient, mockConfig.Object);

        // Act
        var result = await provider.GetResponseAsync("test prompt");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Name_ShouldReturnProviderName()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:GeminiKey"]).Returns("test-key");
        var httpClient = new HttpClient();
        var provider = new GeminiProvider(httpClient, mockConfig.Object);

        // Act
        var name = provider.Name;

        // Assert
        Assert.Equal("Gemini", name);
    }
}
