using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FebroFlow.Business.Services;

public interface IOpenAiService
{
    Task<string> GetChatCompletionAsync(string prompt);
    Task<string> SendPromptAsync(string systemPrompt, string context, string question);
    Task<string> SendPromptAsync(string systemPrompt, string context, string question, Dictionary<string, string> chatContext);
}




public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public OpenAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];
        _model = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo"; // default fallback
    }

    public async Task<string> GetChatCompletionAsync(string prompt)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "Ты помощник, основанный на предоставленном контексте." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7
        };

        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseString);
        var result = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return result ?? string.Empty;
    }

    public async Task<string> SendPromptAsync(string systemPrompt, string context, string question)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt + "----------------\n{context}" },
                new { role = "user", content = context },
                new { role = "user", content = question }
            },
            temperature = 0.7
        };
            
        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseString);
        var result = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        
        return result ?? string.Empty;
    }

    public async Task<string> SendPromptAsync(string systemPrompt, string context, string question, Dictionary<string, string> chatContext)
    {
        var messages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };
                        
        if (!string.IsNullOrWhiteSpace(context))
        {
            messages.Add(new { role = "user", content = context });
        }

        foreach (var pair in chatContext)
        {
            messages.Add(new { role = "user", content = pair.Key });
            messages.Add(new { role = "assistant", content = pair.Value });
        }

        messages.Add(new { role = "user", content = question });
        
        var requestBody = new
        {
            model = _model,
            messages = messages,
            temperature = 0.7
        };
        
        var requestJson = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
        
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseString);
        var result = document.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
        
        return result ?? string.Empty;
    }
}
