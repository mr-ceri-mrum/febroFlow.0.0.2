using febroFlow.Core.Models.OpenAI;

namespace febroFlow.Business.Services;

/// <summary>
/// Interface for interacting with OpenAI services
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Generate a chat completion from OpenAI
    /// </summary>
    /// <param name="messages">The messages to send to OpenAI</param>
    /// <param name="model">The model to use (defaults to gpt-4-turbo)</param>
    /// <param name="temperature">The sampling temperature (defaults to 0.7)</param>
    /// <param name="maxTokens">The maximum number of tokens to generate (optional)</param>
    /// <returns>The response from OpenAI</returns>
    Task<ChatCompletionResponse> CreateChatCompletion(
        List<ChatMessage> messages, 
        string model = "gpt-4-turbo", 
        float temperature = 0.7f,
        int? maxTokens = null);
    
    /// <summary>
    /// Generate an embedding for text
    /// </summary>
    /// <param name="text">The text to embed</param>
    /// <param name="model">The model to use for embeddings (defaults to text-embedding-3-small)</param>
    /// <returns>The embedding vector</returns>
    Task<List<float>> CreateEmbedding(string text, string model = "text-embedding-3-small");
    
    /// <summary>
    /// Generate an image using DALL-E
    /// </summary>
    /// <param name="prompt">The image prompt</param>
    /// <param name="size">Image size (defaults to 1024x1024)</param>
    /// <param name="quality">Image quality (defaults to standard)</param>
    /// <returns>Image URL</returns>
    Task<string> CreateImage(string prompt, string size = "1024x1024", string quality = "standard");
    
    /// <summary>
    /// Moderate content using OpenAI's moderation API
    /// </summary>
    /// <param name="input">The content to moderate</param>
    /// <returns>Moderation results</returns>
    Task<ModerationResponse> CreateModeration(string input);
}