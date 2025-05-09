using FebroFlow.Core.ResultResponses;
using FebroFlow.Business.ServiceRegistrations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace FebroFlow.Business.Services;

public interface IOpenAIService
{
    /// <summary>
    /// Generates a completion using OpenAI API
    /// </summary>
    Task<IDataResult<string>> GenerateCompletionAsync(string prompt, string model = "gpt-4o", float temperature = 0.7f);
    
    /// <summary>
    /// Generates embeddings for text
    /// </summary>
    Task<IDataResult<float[]>> GenerateEmbeddingsAsync(string text, string model = "text-embedding-3-small");
    
    /// <summary>
    /// Transcribes audio to text
    /// </summary>
    Task<IDataResult<string>> TranscribeAudioAsync(byte[] audioData, string language = "en");
    
    /// <summary>
    /// Analyzes an image
    /// </summary>
    Task<IDataResult<string>> AnalyzeImageAsync(byte[] imageData, string prompt);
}

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _organization;
    
    public OpenAIService(HttpClient httpClient, IOptions<IntegrationServiceRegistrations.OpenAIOptions> options)
    {
        _httpClient = httpClient;
        _apiKey = options.Value.ApiKey;
        _organization = options.Value.Organization;
        
        // Set default headers for API requests
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        if (!string.IsNullOrEmpty(_organization))
        {
            _httpClient.DefaultRequestHeaders.Add("OpenAI-Organization", _organization);
        }
    }

    public async Task<IDataResult<string>> GenerateCompletionAsync(string prompt, string model = "gpt-4o", float temperature = 0.7f)
    {
        try
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            
            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = 500
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<dynamic>(responseContent);
                
                // This is a simplified extraction - in a real implementation,
                // properly parse the JSON and extract the completion
                string completion = responseContent.Contains("content") ? "Extracted completion" : "No completion found";
                
                return new SuccessDataResult<string>(completion, "Completion generated successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<string>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<string>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<float[]>> GenerateEmbeddingsAsync(string text, string model = "text-embedding-3-small")
    {
        try
        {
            string apiUrl = "https://api.openai.com/v1/embeddings";
            
            var requestBody = new
            {
                model = model,
                input = text
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // This is a placeholder - in a real implementation,
                // properly parse the JSON and extract the embedding vector
                float[] embedding = new float[1536]; // Example dimension for OpenAI embeddings
                
                return new SuccessDataResult<float[]>(embedding, "Embedding generated successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<float[]>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<float[]>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> TranscribeAudioAsync(byte[] audioData, string language = "en")
    {
        try
        {
            string apiUrl = "https://api.openai.com/v1/audio/transcriptions";
            
            // Create multipart form content
            using var formContent = new MultipartFormDataContent();
            
            // Add the audio file
            var audioContent = new ByteArrayContent(audioData);
            formContent.Add(audioContent, "file", "audio.mp3");
            
            // Add other required fields
            formContent.Add(new StringContent("whisper-1"), "model");
            formContent.Add(new StringContent(language), "language");
            
            var response = await _httpClient.PostAsync(apiUrl, formContent);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<dynamic>(responseContent);
                
                // This is a simplified extraction - in a real implementation,
                // properly parse the JSON and extract the transcription
                string transcription = responseContent.Contains("text") ? "Extracted transcription" : "No transcription found";
                
                return new SuccessDataResult<string>(transcription, "Audio transcribed successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<string>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<string>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> AnalyzeImageAsync(byte[] imageData, string prompt)
    {
        try
        {
            string apiUrl = "https://api.openai.com/v1/chat/completions";
            
            // Convert image to base64
            string base64Image = Convert.ToBase64String(imageData);
            
            // Create request body with image
            var requestBody = new
            {
                model = "gpt-4-vision-preview",
                messages = new[]
                {
                    new 
                    { 
                        role = "user", 
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new 
                            { 
                                type = "image_url", 
                                image_url = new
                                {
                                    url = $"data:image/jpeg;base64,{base64Image}"
                                }
                            }
                        }
                    }
                },
                max_tokens = 300
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<dynamic>(responseContent);
                
                // This is a simplified extraction - in a real implementation,
                // properly parse the JSON and extract the analysis
                string analysis = responseContent.Contains("content") ? "Extracted image analysis" : "No analysis found";
                
                return new SuccessDataResult<string>(analysis, "Image analyzed successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<string>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<string>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }
}