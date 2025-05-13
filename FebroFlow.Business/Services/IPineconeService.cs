using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FebroFlow.Core.Responses;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Embeddings;

namespace FebroFlow.Business.Services;

/// <summary>
/// Интерфейс для взаимодействия с векторной базой данных Pinecone
/// </summary>
public interface IPineconeService
{
    /// <summary>
    /// Добавление вектора в базу данных
    /// </summary>
    /// <param name="id">Идентификатор вектора</param>
    /// <param name="vector">Вектор</param>
    /// <param name="metadata">Метаданные</param>
    /// <param name="nameSpace">Namespace для вектора</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpsertVectorAsync(string id, float[] vector, Dictionary<string, object> metadata, string nameSpace = "");
    
    /// <summary>
    /// Добавление векторов в базу данных пакетно
    /// </summary>
    /// <param name="vectors">Словарь векторов: ключ - id, значение - массив координат</param>
    /// <param name="metadata">Словарь метаданных для каждого вектора</param>
    /// <param name="nameSpace">Namespace для векторов</param>
    /// <returns>Результат операции</returns>
    Task<bool> UpsertVectorsAsync(Dictionary<string, float[]> vectors, Dictionary<string, Dictionary<string, object>> metadata, string nameSpace = "");
    
    /// <summary>
    /// Поиск ближайших векторов
    /// </summary>
    /// <param name="vector">Вектор для поиска</param>
    /// <param name="topK">Количество ближайших векторов для возврата</param>
    /// <param name="nameSpace">Namespace для поиска</param>
    /// <param name="filter">Фильтр для поиска</param>
    /// <returns>Результаты поиска с метаданными</returns>
    Task<List<PineconeSearchResult>> QueryAsync(float[] vector, int topK = 10, string index = "");
    
    /// <summary>
    /// Удаление вектора из базы данных
    /// </summary>
    /// <param name="id">Идентификатор вектора</param>
    /// <param name="nameSpace">Namespace вектора</param>
    /// <returns>Результат операции</returns>
    Task<bool> DeleteVectorAsync(string id, string nameSpace = "");
    
    /// <summary>
    /// Удаление множества векторов с фильтрацией
    /// </summary>
    /// <param name="filter">Фильтр для удаления</param>
    /// <param name="nameSpace">Namespace для удаления</param>
    /// <returns>Результат операции</returns>
    Task<bool> DeleteVectorsAsync(Dictionary<string, object> filter, string nameSpace = "");

    Task<IResult> CreateIndexAsync(string indexName, int dimension, string metric);
    Task<object> ListIndexesAsync();
    
    Task<List<float>> GenerateEmbeddingAsync(string text);
}

public class PineconeService(OpenAIClient openAiClient, IConfiguration configuration) : IPineconeService
{
   
    
    public Task<bool> UpsertVectorAsync(string id, float[] vector, Dictionary<string, object> metadata, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpsertVectorsAsync(Dictionary<string, float[]> vectors, Dictionary<string, Dictionary<string, object>> metadata, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public async Task<List<PineconeSearchResult>> QueryAsync(float[] vector, int topK = 10, string index = "")
    {
        // тут будет url динасическим
        var url = $"https://royalflauwers2-c3yzq0e.svc.aped-4627-b74a.pinecone.io"; // замените на ваш endpoint
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Api-Key", "pcsk_5oHWE8_M23LHLy2z4cosTBkAwjzUMkD6N2M56uocTobTvbqEqfV3iV6fTQRtgHAzPnAfCV"); // подставь свой API-ключ
        var requestBody = new
        {
            vector,
            topK,
            includeMetadata = true,
            includeValues = false
        };
        
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );
        var response = await httpClient.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Pinecone query failed: {error}");
        }
        var responseString = await response.Content.ReadAsStringAsync();
        
        return JsonSerializer.Deserialize<List<PineconeSearchResult>>(responseString) ?? throw new InvalidOperationException();
    }

    public Task<bool> DeleteVectorAsync(string id, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteVectorsAsync(Dictionary<string, object> filter, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<IResult> CreateIndexAsync(string indexName, int dimension, string metric)
    {
        throw new NotImplementedException();
    }

    public Task<object> ListIndexesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        var requestBody = new
        {
            input = text, model
        };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        
        var responseContent = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseContent);

        var embeddingArray = doc.RootElement
            .GetProperty("data")[0]
            .GetProperty("embedding");
        
        var result = new float[embeddingArray.GetArrayLength()];
        var index = 0;

        foreach (var value in embeddingArray.EnumerateArray())
        {
            result[index++] = value.GetSingle();
        }

        return result.ToList();
    }
}

/// <summary>
/// Класс для результатов поиска в Pinecone
/// </summary>
public class PineconeSearchResult
{
    /// <summary>
    /// Идентификатор вектора
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Значение сходства (от 0 до 1)
    /// </summary>
    public float Score { get; set; }
    
    /// <summary>
    /// Метаданные вектора
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; }
}