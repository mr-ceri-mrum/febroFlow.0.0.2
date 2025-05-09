using FebroFlow.Core.ResultResponses;
using FebroFlow.Business.ServiceRegistrations;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FebroFlow.Business.Services;

public interface ITelegramService
{
    /// <summary>
    /// Sets up a webhook for receiving Telegram updates
    /// </summary>
    Task<IDataResult<bool>> SetWebhookAsync(string url);
    
    /// <summary>
    /// Removes the webhook
    /// </summary>
    Task<IDataResult<bool>> DeleteWebhookAsync();
    
    /// <summary>
    /// Sends a text message to a Telegram chat
    /// </summary>
    Task<IDataResult<object>> SendMessageAsync(string chatId, string text, int? replyToMessageId = null);
    
    /// <summary>
    /// Processes an incoming webhook update from Telegram
    /// </summary>
    Task<IDataResult<object>> ProcessUpdateAsync(string updateJson);
}

public class TelegramService : ITelegramService
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly string _webhookUrl;
    
    public TelegramService(HttpClient httpClient, IOptions<IntegrationServiceRegistrations.TelegramOptions> options)
    {
        _httpClient = httpClient;
        _botToken = options.Value.BotToken;
        _webhookUrl = options.Value.WebhookUrl;
    }

    public async Task<IDataResult<bool>> SetWebhookAsync(string url)
    {
        try
        {
            string apiUrl = $"https://api.telegram.org/bot{_botToken}/setWebhook?url={url}";
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return new SuccessDataResult<bool>(true, "Webhook set successfully");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<bool>(responseContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<bool>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<bool>> DeleteWebhookAsync()
    {
        try
        {
            string apiUrl = $"https://api.telegram.org/bot{_botToken}/deleteWebhook";
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return new SuccessDataResult<bool>(true, "Webhook deleted successfully");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<bool>(responseContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<bool>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<object>> SendMessageAsync(string chatId, string text, int? replyToMessageId = null)
    {
        try
        {
            string apiUrl = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            
            var content = new Dictionary<string, string>
            {
                { "chat_id", chatId },
                { "text", text },
                { "parse_mode", "Markdown" },
            };
            
            if (replyToMessageId.HasValue)
            {
                content.Add("reply_to_message_id", replyToMessageId.Value.ToString());
            }
            
            var response = await _httpClient.PostAsync(apiUrl, new FormUrlEncodedContent(content));
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<object>(responseContent);
                return new SuccessDataResult<object>(responseObj!, "Message sent successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<object>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<object>> ProcessUpdateAsync(string updateJson)
    {
        try
        {
            // Parse update
            var update = JsonSerializer.Deserialize<object>(updateJson);
            
            // Simplified implementation - in reality you would parse the update,
            // extract the message, analyze content, etc.
            
            return new SuccessDataResult<object>(update!, "Update processed successfully");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }
}