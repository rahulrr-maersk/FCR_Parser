namespace FcrParser.Services.AI;

public interface IAIProvider
{
    string Name { get; }
    
    // Generic method: takes a string prompt, returns the raw AI response string
    Task<string?> GetResponseAsync(string prompt);
}