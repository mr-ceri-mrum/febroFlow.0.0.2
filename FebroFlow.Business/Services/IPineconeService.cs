using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FebroFlow.Core.Responses;
using Microsoft.Extensions.Configuration;
using OpenAI;
using Pinecone;

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
    /// <param name="collectionName"></param>
    /// <param name="topK">Количество ближайших векторов для возврата</param>
    /// <returns>Результаты поиска с метаданными</returns>
    Task<JsonElement> QueryAsync(float[] vector, string collectionName,int topK = 10);
    
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

    Task CreateIndexAsync(string indexName);
    Task<object> ListIndexesAsync();
    
    Task<List<float>> GenerateEmbeddingAsync(string text);
}

public class PineconeService(OpenAIClient openAiClient, IConfiguration configuration, HttpClient httpClient) : IPineconeService
{
    private readonly PineconeClient _pineconeClient = new(configuration.GetValue<string>("Pinecone:ApiKey") ?? throw new InvalidOperationException());
    private const string endpoint = "https://api.openai.com/v1/embeddings";
    private const string model = "text-embedding-3-small";
    private readonly string _apiKeyOpenAi = configuration.GetValue<string>("OpenAI:ApiKey") ?? throw new InvalidOperationException();
    public Task<bool> UpsertVectorAsync(string id, float[] vector, Dictionary<string, object> metadata, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpsertVectorsAsync(Dictionary<string, float[]> vectors, Dictionary<string, Dictionary<string, object>> metadata, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<JsonElement> QueryAsync(float[] vector, string collectionName, int topK = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<JsonElement> QueryAsync(float[] vector, string collectionName,  int topK = 10, string collectionNameSpace = "") 
    {
        var json = JsonSerializer.Serialize(collectionName);
        var document = JsonDocument.Parse(json);
        return document.RootElement;
    }
 

    public Task<bool> DeleteVectorAsync(string id, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteVectorsAsync(Dictionary<string, object> filter, string nameSpace = "")
    {
        throw new NotImplementedException();
    }

    public async Task CreateIndexAsync(string indexName)
    {
        await _pineconeClient.CreateIndexAsync(new CreateIndexRequest
        {
            Name = indexName,
            VectorType = VectorType.Dense,
            Dimension = 1536,
            Metric = CreateIndexRequestMetric.Cosine,
            Spec = new ServerlessIndexSpec
            {
                Serverless = new ServerlessSpec
                {
                    Cloud = ServerlessSpecCloud.Aws,
                    Region = "us-east-1"
                }
            },
            DeletionProtection = DeletionProtection.Disabled,
            Tags = new Dictionary<string, string> 
            {  
                { "environment", "development" }
            }
        });
    }

    public Task<object> ListIndexesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<float>> GenerateEmbeddingAsync(string text)
    {
        string endpoint = "https://api.openai.com/v1/embeddings";
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKeyOpenAi);
        
        var requestBody = new
        {
            input = text,
            model = "text-embedding-3-small"
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


