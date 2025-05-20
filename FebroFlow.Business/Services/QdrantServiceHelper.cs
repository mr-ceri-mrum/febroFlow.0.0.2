using System.Text;
using System.Text.Json;
using FebroFlow.Data.Dtos.Vector;

namespace FebroFlow.Business.Services;

public class QdrantCollectionResponse
{
    public QdrantResult Result { get; set; }
    public string Status { get; set; }
    public double Time { get; set; }
}

public class QdrantResult
{
    public List<CollectionDescription> Collections { get; set; }
}

public class CollectionDescription
{
    public string Name { get; set; }
}



public class QdrantServiceHelper
{
    private readonly HttpClient _httpClient;

    public QdrantServiceHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:6333");
    }
    
    public async Task<List<string>> GetAllCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/collections", cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine(json); // Это строка: {"result":{"collections":[{"name":"..."},...]}, "status":"ok", "time":...}

        var result = JsonSerializer.Deserialize<QdrantCollectionResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        return result?.Result?.Collections?.Select(c => c.Name).ToList() ?? new List<string>();
        
    }
    
    public async Task UpsertPointAsync(string collectionName, QdrantPoint point,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            points = new[]
            {
                new
                {
                    id = point.Id,
                    vector = point.Vector,
                    payload = point.Payload
                }
            }
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
            
        await _httpClient.PutAsync($"/collections/{collectionName}/points", content, cancellationToken);
    }
    
    public async Task<List<QdrantSearchResultDto>> SearchAsync(string collectionName, List<float> queryVector, int limit = 5, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            vector = queryVector,
            limit = limit,
            with_payload = true
        };
        
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"/collections/{collectionName}/points/search", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var document = JsonDocument.Parse(json);
        var results = new List<QdrantSearchResultDto>();
        
        foreach (var point in document.RootElement.GetProperty("result").EnumerateArray())
        {
            var id = point.GetProperty("id").ToString();
            var score = point.GetProperty("score").GetDouble();

            Dictionary<string, object>? payload = null;
            if (point.TryGetProperty("payload", out var payloadElement))
            {
                payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadElement.GetRawText());
            }

            results.Add(new QdrantSearchResultDto
            {
                Id = id,
                Score = score,
                Payload = payload
            });
        }
        
        return results;
    }

}

public class QdrantSearchResultDto
{
    public string Id { get; set; }
    public double Score { get; set; }
    public Dictionary<string, object> Payload { get; set; }
}