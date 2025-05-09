using FebroFlow.Data.Dtos.AI;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для взаимодействия с API OpenAI
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Отправка запроса на генерацию текста
    /// </summary>
    /// <param name="request">Запрос на генерацию</param>
    /// <returns>Ответ с сгенерированным текстом</returns>
    Task<TextCompletionResponse> GenerateTextAsync(TextCompletionRequest request);
    
    /// <summary>
    /// Отправка запроса на генерацию текста в потоковом режиме
    /// </summary>
    /// <param name="request">Запрос на генерацию</param>
    /// <returns>Поток ответов с сгенерированным текстом</returns>
    IAsyncEnumerable<TextCompletionStreamResponse> GenerateTextStreamAsync(TextCompletionRequest request);
    
    /// <summary>
    /// Отправка запроса на генерацию эмбеддингов для текста
    /// </summary>
    /// <param name="request">Запрос на генерацию эмбеддингов</param>
    /// <returns>Ответ с эмбеддингами</returns>
    Task<EmbeddingResponse> GenerateEmbeddingsAsync(EmbeddingRequest request);
    
    /// <summary>
    /// Получение доступных моделей
    /// </summary>
    /// <returns>Список доступных моделей</returns>
    Task<List<ModelInfo>> GetAvailableModelsAsync();
    
    /// <summary>
    /// Проверка доступности API
    /// </summary>
    /// <returns>Статус доступности</returns>
    Task<bool> CheckApiAvailabilityAsync();
}