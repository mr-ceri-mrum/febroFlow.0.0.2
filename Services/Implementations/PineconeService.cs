using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using febroFlow.Services;

namespace febroFlow.Services.Implementations
{
    /// <summary>
    /// Implementation of IPineconeService that interacts with the Pinecone Vector Database API
    /// </summary>
    public class PineconeService : IPineconeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PineconeService> _logger;
        private readonly string _baseUrl;
        private readonly string _indexName;
        private readonly string _projectId;
        
        public PineconeService(IConfiguration configuration, ILogger<PineconeService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            
            var apiKey = configuration["Pinecone:ApiKey"] 
                ?? throw new ArgumentNullException("Pinecone:ApiKey", "Pinecone API key is not configured");
            
            _indexName = configuration["Pinecone:IndexName"] 
                ?? throw new ArgumentNullException("Pinecone:IndexName", "Pinecone index name is not configured");
            
            _projectId = configuration["Pinecone:ProjectId"] 
                ?? throw new ArgumentNullException("Pinecone:ProjectId", "Pinecone project ID is not configured");
            
            var environment = configuration["Pinecone:Environment"] ?? "us-west1-gcp";
            
            _baseUrl = $"https://{_indexName}-{_projectId}.svc.{environment}.pinecone.io";
            
            _httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <inheritdoc/>
        public async Task<bool> UpsertVectorAsync(string vectorId, float[] vector, Dictionary<string, object> metadata)
        {
            try
            {
                var endpoint = $"{_baseUrl}/vectors/upsert";
                
                var request = new
                {
                    vectors = new[]
                    {
                        new
                        {
                            id = vectorId,
                            values = vector,
                            metadata
                        }
                    }
                };
                
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upserting vector to Pinecone: {Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<SearchResult>> QueryAsync(float[] vector, int topK = 10)
        {
            try
            {
                var endpoint = $"{_baseUrl}/query";
                
                var request = new
                {
                    vector,
                    topK,
                    includeMetadata = true
                };
                
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var queryResponse = JsonSerializer.Deserialize<PineconeQueryResponse>(responseContent, options);
                
                if (queryResponse?.Matches == null)
                {
                    return new List<SearchResult>();
                }
                
                return queryResponse.Matches.Select(match => new SearchResult
                {
                    Id = match.Id,
                    Score = match.Score,
                    Metadata = match.Metadata
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Pinecone: {Message}", ex.Message);
                return new List<SearchResult>();
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteVectorAsync(string vectorId)
        {
            try
            {
                var endpoint = $"{_baseUrl}/vectors/delete";
                
                var request = new
                {
                    ids = new[] { vectorId }
                };
                
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vector from Pinecone: {Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> FetchVectorAsync(string vectorId)
        {
            try
            {
                var endpoint = $"{_baseUrl}/vectors/fetch?ids={vectorId}";
                
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var fetchResponse = JsonSerializer.Deserialize<PineconeFetchResponse>(responseContent, options);
                
                if (fetchResponse?.Vectors == null || !fetchResponse.Vectors.ContainsKey(vectorId))
                {
                    return new Dictionary<string, object>();
                }
                
                return fetchResponse.Vectors[vectorId].Metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vector from Pinecone: {Message}", ex.Message);
                return new Dictionary<string, object>();
            }
        }
        
        // Helper classes for deserializing Pinecone responses
        private class PineconeQueryResponse
        {
            public List<PineconeMatch> Matches { get; set; }
        }
        
        private class PineconeMatch
        {
            public string Id { get; set; }
            public float Score { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }
        
        private class PineconeFetchResponse
        {
            public Dictionary<string, PineconeVector> Vectors { get; set; }
        }
        
        private class PineconeVector
        {
            public string Id { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }
    }
}