using System.Text;
using FebroFlow.Core.ResultResponses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for Azure OpenAI Service integration
/// </summary>
public interface IAzureOpenAIService
{
    /// <summary>
    /// Generate embeddings for text using Azure OpenAI
    /// </summary>
    /// <param name="text">The text to generate embeddings for</param>
    /// <returns>A list of floating point values representing the embeddings</returns>
    Task<IDataResult<float[]>> GenerateEmbeddingsAsync(string text);
    
    /// <summary>
    /// Generate chat completions using Azure OpenAI
    /// </summary>
    /// <param name="messages">List of messages in the conversation history</param>
    /// <param name="temperature">Controls randomness (0.0 to 1.0)</param>
    /// <param name="maxTokens">Maximum tokens to generate</param>
    /// <param name="model">Optional model to use, defaults to configuration setting</param>
    /// <returns>Response from the AI model</returns>
    Task<IDataResult<string>> GenerateChatCompletionAsync(List<ChatMessage> messages, 
        float temperature = 0.7f, 
        int? maxTokens = null,
        string? model = null);
    
    /// <summary>
    /// Generate an image from a text prompt using Azure OpenAI DALL-E
    /// </summary>
    /// <param name="prompt">The prompt to generate the image from</param>
    /// <param name="size">Image size (e.g., "1024x1024")</param>
    /// <param name="n">Number of images to generate</param>
    /// <returns>URL(s) to the generated image(s)</returns>
    Task<IDataResult<List<string>>> GenerateImageAsync(string prompt, string size = "1024x1024", int n = 1);
    
    /// <summary>
    /// Transcribe audio to text using Azure OpenAI Whisper
    /// </summary>
    /// <param name="audioData">The audio data as a byte array</param>
    /// <param name="prompt">Optional prompt to guide transcription</param>
    /// <returns>Transcribed text</returns>
    Task<IDataResult<string>> TranscribeAudioAsync(byte[] audioData, string? prompt = null);
}

/// <summary>
/// ChatMessage structure for Azure OpenAI API
/// </summary>
public class ChatMessage
{
    [JsonProperty("role")]
    public string Role { get; set; }
    
    [JsonProperty("content")]
    public string Content { get; set; }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}

/// <summary>
/// Implementation of Azure OpenAI Service
/// </summary>
public class AzureOpenAIService : IAzureOpenAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureOpenAIService> _logger;

    public AzureOpenAIService(IHttpClientFactory httpClientFactory, 
                              IConfiguration configuration,
                              ILogger<AzureOpenAIService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IDataResult<float[]>> GenerateEmbeddingsAsync(string text)
    {
        try
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string endpoint = _configuration["AzureOpenAI:EmbeddingsEndpoint"];
            string deploymentName = _configuration["AzureOpenAI:EmbeddingsDeployment"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
            {
                return new ErrorDataResult<float[]>("Azure OpenAI configuration is incomplete", System.Net.HttpStatusCode.BadRequest);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/embeddings?api-version=2023-05-15";
            
            var request = new
            {
                input = text,
                model = "text-embedding-ada-002"
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Azure OpenAI API error: {errorContent}");
                return new ErrorDataResult<float[]>($"Azure OpenAI API error: {response.StatusCode}", System.Net.HttpStatusCode.BadRequest);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<dynamic>(jsonString);
            var embeddings = responseObj?.data[0]?.embedding.ToObject<float[]>();

            if (embeddings == null)
            {
                return new ErrorDataResult<float[]>("Failed to parse embeddings from response", System.Net.HttpStatusCode.InternalServerError);
            }

            return new SuccessDataResult<float[]>(embeddings, "Embeddings generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            return new ErrorDataResult<float[]>($"Error generating embeddings: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> GenerateChatCompletionAsync(List<ChatMessage> messages, 
        float temperature = 0.7f, 
        int? maxTokens = null,
        string? model = null)
    {
        try
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string endpoint = _configuration["AzureOpenAI:CompletionsEndpoint"];
            string deploymentName = model ?? _configuration["AzureOpenAI:ChatCompletionsDeployment"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
            {
                return new ErrorDataResult<string>("Azure OpenAI configuration is incomplete", System.Net.HttpStatusCode.BadRequest);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/chat/completions?api-version=2023-05-15";
            
            var request = new
            {
                messages = messages,
                temperature = temperature,
                max_tokens = maxTokens
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Azure OpenAI API error: {errorContent}");
                return new ErrorDataResult<string>($"Azure OpenAI API error: {response.StatusCode}", System.Net.HttpStatusCode.BadRequest);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<dynamic>(jsonString);
            string completionText = responseObj?.choices[0]?.message?.content;

            if (string.IsNullOrEmpty(completionText))
            {
                return new ErrorDataResult<string>("Failed to parse completion from response", System.Net.HttpStatusCode.InternalServerError);
            }

            return new SuccessDataResult<string>(completionText, "Chat completion generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat completion");
            return new ErrorDataResult<string>($"Error generating chat completion: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<List<string>>> GenerateImageAsync(string prompt, string size = "1024x1024", int n = 1)
    {
        try
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string endpoint = _configuration["AzureOpenAI:ImageGenerationEndpoint"];
            string deploymentName = _configuration["AzureOpenAI:ImageGenerationDeployment"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
            {
                return new ErrorDataResult<List<string>>("Azure OpenAI configuration is incomplete", System.Net.HttpStatusCode.BadRequest);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/images/generations?api-version=2023-06-01-preview";
            
            var request = new
            {
                prompt = prompt,
                n = n,
                size = size
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(requestUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Azure OpenAI API error: {errorContent}");
                return new ErrorDataResult<List<string>>($"Azure OpenAI API error: {response.StatusCode}", System.Net.HttpStatusCode.BadRequest);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<dynamic>(jsonString);
            
            var imageUrls = new List<string>();
            foreach (var data in responseObj?.data)
            {
                string url = data?.url;
                if (!string.IsNullOrEmpty(url))
                {
                    imageUrls.Add(url);
                }
            }

            if (imageUrls.Count == 0)
            {
                return new ErrorDataResult<List<string>>("Failed to generate any images", System.Net.HttpStatusCode.InternalServerError);
            }

            return new SuccessDataResult<List<string>>(imageUrls, "Images generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating images");
            return new ErrorDataResult<List<string>>($"Error generating images: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<string>> TranscribeAudioAsync(byte[] audioData, string? prompt = null)
    {
        try
        {
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string endpoint = _configuration["AzureOpenAI:TranscriptionEndpoint"];
            string deploymentName = _configuration["AzureOpenAI:TranscriptionDeployment"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(deploymentName))
            {
                return new ErrorDataResult<string>("Azure OpenAI configuration is incomplete", System.Net.HttpStatusCode.BadRequest);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("api-key", apiKey);

            var requestUrl = $"{endpoint}openai/deployments/{deploymentName}/audio/transcriptions?api-version=2023-09-01-preview";
            
            using var formContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(audioData);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mpeg");
            formContent.Add(fileContent, "file", "audio.mp3");
            formContent.Add(new StringContent("whisper-1"), "model");
            
            if (!string.IsNullOrEmpty(prompt))
            {
                formContent.Add(new StringContent(prompt), "prompt");
            }

            var response = await client.PostAsync(requestUrl, formContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Azure OpenAI API error: {errorContent}");
                return new ErrorDataResult<string>($"Azure OpenAI API error: {response.StatusCode}", System.Net.HttpStatusCode.BadRequest);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseObj = JsonConvert.DeserializeObject<dynamic>(jsonString);
            string transcription = responseObj?.text;

            if (string.IsNullOrEmpty(transcription))
            {
                return new ErrorDataResult<string>("Failed to transcribe audio", System.Net.HttpStatusCode.InternalServerError);
            }

            return new SuccessDataResult<string>(transcription, "Audio transcribed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio");
            return new ErrorDataResult<string>($"Error transcribing audio: {ex.Message}", System.Net.HttpStatusCode.InternalServerError);
        }
    }
}