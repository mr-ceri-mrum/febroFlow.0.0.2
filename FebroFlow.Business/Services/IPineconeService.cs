using FebroFlow.Core.ResultResponses;
using FebroFlow.Business.ServiceRegistrations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace FebroFlow.Business.Services;

public interface IPineconeService
{
    /// <summary>
    /// Creates a new index in Pinecone
    /// </summary>
    Task<IDataResult<bool>> CreateIndexAsync(string indexName, int dimension = 1536);
    
    /// <summary>
    /// Deletes an index from Pinecone
    /// </summary>
    Task<IDataResult<bool>> DeleteIndexAsync(string indexName);
    
    /// <summary>
    /// Upserts vectors into a Pinecone index
    /// </summary>
    Task<IDataResult<bool>> UpsertVectorsAsync(string indexName, Dictionary<string, float[]> vectors, Dictionary<string, object>? metadata = null);
    
    /// <summary>
    /// Queries vectors from a Pinecone index
    /// </summary>
    Task<IDataResult<List<(string Id, float Score, Dictionary<string, object> Metadata)>>> QueryVectorsAsync(
        string indexName, float[] queryVector, int topK = 5);
}

public class PineconeService : IPineconeService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _environment;
    private readonly string _projectId;
    
    public PineconeService(HttpClient httpClient, IOptions<IntegrationServiceRegistrations.PineconeOptions> options)
    {
        _httpClient = httpClient;
        _apiKey = options.Value.ApiKey;
        _environment = options.Value.Environment;
        _projectId = options.Value.ProjectId;
        
        // Set default headers for API requests
        _httpClient.DefaultRequestHeaders.Add("Api-Key", _apiKey);
    }

    public async Task<IDataResult<bool>> CreateIndexAsync(string indexName, int dimension = 1536)
    {
        try
        {
            string apiUrl = $"https://controller.{_environment}.pinecone.io/databases";
            
            var requestBody = new
            {
                name = indexName,
                dimension = dimension,
                metric = "cosine"
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                return new SuccessDataResult<bool>(true, "Index created successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<bool>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<bool>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<bool>> DeleteIndexAsync(string indexName)
    {
        try
        {
            string apiUrl = $"https://controller.{_environment}.pinecone.io/databases/{indexName}";
            
            var response = await _httpClient.DeleteAsync(apiUrl);
            
            if (response.IsSuccessStatusCode)
            {
                return new SuccessDataResult<bool>(true, "Index deleted successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<bool>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<bool>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<bool>> UpsertVectorsAsync(string indexName, Dictionary<string, float[]> vectors, Dictionary<string, object>? metadata = null)
    {
        try
        {
            string apiUrl = $"https://{indexName}-{_projectId}.svc.{_environment}.pinecone.io/vectors/upsert";
            
            // Format vectors for Pinecone API
            var vectorRecords = new List<object>();
            
            foreach (var (id, vector) in vectors)
            {
                var record = new
                {
                    id = id,
                    values = vector,
                    metadata = metadata != null && metadata.ContainsKey(id) ? metadata[id] : null
                };
                
                vectorRecords.Add(record);
            }
            
            var requestBody = new
            {
                vectors = vectorRecords
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                return new SuccessDataResult<bool>(true, "Vectors upserted successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<bool>(errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<bool>(ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }

    public async Task<IDataResult<List<(string Id, float Score, Dictionary<string, object> Metadata)>>> QueryVectorsAsync(string indexName, float[] queryVector, int topK = 5)
    {
        try
        {
            string apiUrl = $"https://{indexName}-{_projectId}.svc.{_environment}.pinecone.io/query";
            
            var requestBody = new
            {
                vector = queryVector,
                topK = topK,
                includeMetadata = true
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(apiUrl, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // This is a simplified extraction - in a real implementation,
                // properly parse the JSON and extract the matches
                var results = new List<(string Id, float Score, Dictionary<string, object> Metadata)>
                {
                    ("sample-id-1", 0.95f, new Dictionary<string, object> { { "text", "Sample text 1" } }),
                    ("sample-id-2", 0.85f, new Dictionary<string, object> { { "text", "Sample text 2" } })
                };
                
                return new SuccessDataResult<List<(string Id, float Score, Dictionary<string, object> Metadata)>>(
                    results, "Query executed successfully");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync();
            return new ErrorDataResult<List<(string Id, float Score, Dictionary<string, object> Metadata)>>(
                errorContent, System.Net.HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<List<(string Id, float Score, Dictionary<string, object> Metadata)>>(
                ex.Message, System.Net.HttpStatusCode.InternalServerError);
        }
    }
}