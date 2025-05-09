using febroFlow.Core.Models.Telegram;

namespace febroFlow.Business.Services;

/// <summary>
/// Interface for Telegram Bot API interactions
/// </summary>
public interface ITelegramService
{
    /// <summary>
    /// Set webhook for Telegram Bot
    /// </summary>
    /// <param name="url">Webhook URL</param>
    /// <param name="allowedUpdates">Types of updates to receive</param>
    /// <returns>Success status</returns>
    Task<bool> SetWebhook(string url, List<string>? allowedUpdates = null);
    
    /// <summary>
    /// Delete webhook for Telegram Bot
    /// </summary>
    /// <returns>Success status</returns>
    Task<bool> DeleteWebhook();
    
    /// <summary>
    /// Get webhook info for Telegram Bot
    /// </summary>
    /// <returns>Webhook info</returns>
    Task<WebhookInfo> GetWebhookInfo();
    
    /// <summary>
    /// Send a text message to a chat
    /// </summary>
    /// <param name="chatId">Chat ID</param>
    /// <param name="text">Message text</param>
    /// <param name="parseMode">How to parse entities in the message text</param>
    /// <returns>Sent message</returns>
    Task<Message> SendMessage(string chatId, string text, string? parseMode = null);
    
    /// <summary>
    /// Process an update from Telegram
    /// </summary>
    /// <param name="update">The update from Telegram</param>
    /// <returns>Task</returns>
    Task ProcessUpdate(Update update);
    
    /// <summary>
    /// Get information about the bot
    /// </summary>
    /// <returns>Bot information</returns>
    Task<User> GetMe();
}