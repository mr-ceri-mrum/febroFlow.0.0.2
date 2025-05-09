using FebroFlow.Data.Dtos.Telegram;
using FebroFlow.Data.Entities;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для взаимодействия с API Telegram
/// </summary>
public interface ITelegramService
{
    /// <summary>
    /// Отправка сообщения пользователю
    /// </summary>
    /// <param name="chatId">ID чата</param>
    /// <param name="text">Текст сообщения</param>
    /// <param name="parseMode">Режим парсинга (Markdown, HTML)</param>
    /// <param name="replyMarkup">Разметка для ответа (кнопки и т.д.)</param>
    /// <returns>Информация об отправленном сообщении</returns>
    Task<TelegramMessageResponse> SendMessageAsync(long chatId, string text, string parseMode = null, object replyMarkup = null);
    
    /// <summary>
    /// Отправка изображения пользователю
    /// </summary>
    /// <param name="chatId">ID чата</param>
    /// <param name="photoUrl">URL изображения или ID файла</param>
    /// <param name="caption">Подпись к изображению</param>
    /// <param name="parseMode">Режим парсинга (Markdown, HTML)</param>
    /// <param name="replyMarkup">Разметка для ответа (кнопки и т.д.)</param>
    /// <returns>Информация об отправленном сообщении</returns>
    Task<TelegramMessageResponse> SendPhotoAsync(long chatId, string photoUrl, string caption = null, string parseMode = null, object replyMarkup = null);
    
    /// <summary>
    /// Установка webhook для получения обновлений
    /// </summary>
    /// <param name="url">URL для получения обновлений</param>
    /// <param name="certificate">Публичный ключ сертификата</param>
    /// <param name="allowedUpdates">Типы разрешенных обновлений</param>
    /// <returns>Результат операции</returns>
    Task<bool> SetWebhookAsync(string url, byte[] certificate = null, string[] allowedUpdates = null);
    
    /// <summary>
    /// Удаление webhook
    /// </summary>
    /// <returns>Результат операции</returns>
    Task<bool> DeleteWebhookAsync();
    
    /// <summary>
    /// Обработка входящего обновления от Telegram
    /// </summary>
    /// <param name="update">Обновление</param>
    /// <returns>Результат обработки</returns>
    Task<TelegramProcessingResult> ProcessUpdateAsync(TelegramUpdate update);
    
    /// <summary>
    /// Получение информации о боте
    /// </summary>
    /// <returns>Информация о боте</returns>
    Task<TelegramUser> GetMeAsync();
}