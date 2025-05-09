using FebroFlow.Core.ResultResponses;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for Vector Database services (abstraction over specific implementations like Pinecone)
/// </summary>
public interface IVectorDatabaseService
{
    /// <summary>
    /// Initialize or create a vector index if it doesn't exist
    /// </summary>
    /// <param name="indexName">Name of the index to create</param>
    /// <param name="dimension">Vector dimension</param>
    /// <param name="metric">Distance metric to use (cosine, euclidean, dotproduct)</param>
    /// <returns>Result of the operation</returns>
    Task<IResult> CreateIndexAsync(string indexName, int dimension, string metric = "cosine");
    
    /// <summary>
    /// Check if an index exists
    /// </summary>
    /// <param name="indexName">Name of the index to check</param>
    /// <returns>True if the index exists</returns>
    Task<IDataResult<bool>> IndexExistsAsync(string indexName);
    
    /// <summary>
    /// Delete an existing vector index
    /// </summary>
    /// <param name="indexName">Name of the index to delete</param>
    /// <returns>Result of the operation</returns>
    Task<IResult> DeleteIndexAsync(string indexName);
    
    /// <summary>
    /// Upsert vectors into the index
    /// </summary>
    /// <param name="indexName">Name of the index</param>
    /// <param name="vectors">Vectors to upsert</param>
    /// <returns>Result of the operation with number of vectors upserted</returns>
    Task<IDataResult<int>> UpsertVectorsAsync(string indexName, List<VectorRecord> vectors);
    
    /// <summary>
    /// Query vectors by similarity
    /// </summary>
    /// <param name="indexName">Name of the index</param>
    /// <param name="queryVector">Vector to query against</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="filter">Optional filter to apply</param>
    /// <returns>List of similar vectors with scores</returns>
    Task<IDataResult<List<ScoredVectorRecord>>> QueryAsync(string indexName, float[] queryVector, int topK = 10, Dictionary<string, object>? filter = null);
    
    /// <summary>
    /// Delete vectors from the index
    /// </summary>
    /// <param name="indexName">Name of the index</param>
    /// <param name="ids">IDs of vectors to delete</param>
    /// <returns>Result of the operation with number of vectors deleted</returns>
    Task<IDataResult<int>> DeleteVectorsAsync(string indexName, List<string> ids);
    
    /// <summary>
    /// Get vectors by ID
    /// </summary>
    /// <param name="indexName">Name of the index</param>
    /// <param name="ids">IDs of vectors to retrieve</param>
    /// <returns>List of retrieved vectors</returns>
    Task<IDataResult<List<VectorRecord>>> FetchVectorsAsync(string indexName, List<string> ids);
    
    /// <summary>
    /// Get statistics about the index
    /// </summary>
    /// <param name="indexName">Name of the index</param>
    /// <returns>Index statistics</returns>
    Task<IDataResult<IndexStats>> GetIndexStatsAsync(string indexName);
}

/// <summary>
/// Represents a vector record in the database
/// </summary>
public class VectorRecord
{
    /// <summary>
    /// Unique identifier for the vector
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Vector values
    /// </summary>
    public float[] Values { get; set; } = Array.Empty<float>();
    
    /// <summary>
    /// Metadata associated with the vector
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a vector record with similarity score
/// </summary>
public class ScoredVectorRecord : VectorRecord
{
    /// <summary>
    /// Similarity score between query vector and this vector
    /// </summary>
    public float Score { get; set; }
}

/// <summary>
/// Statistics about a vector index
/// </summary>
public class IndexStats
{
    /// <summary>
    /// Name of the index
    /// </summary>
    public string IndexName { get; set; } = string.Empty;
    
    /// <summary>
    /// Dimension of vectors in the index
    /// </summary>
    public int Dimension { get; set; }
    
    /// <summary>
    /// Number of vectors in the index
    /// </summary>
    public long VectorCount { get; set; }
    
    /// <summary>
    /// Index size in bytes
    /// </summary>
    public long IndexSize { get; set; }
    
    /// <summary>
    /// When the index was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Implementation of Vector Database Service that delegates to Pinecone
/// </summary>
public class VectorDatabaseService : IVectorDatabaseService
{
    private readonly IPineconeService _pineconeService;
    
    public VectorDatabaseService(IPineconeService pineconeService)
    {
        _pineconeService = pineconeService;
    }
    
    public async Task<IResult> CreateIndexAsync(string indexName, int dimension, string metric = "cosine")
    {
        return await _pineconeService.CreateIndexAsync(indexName, dimension, metric);
    }
    
    public async Task<IDataResult<bool>> IndexExistsAsync(string indexName)
    {
        var indexesResult = await _pineconeService.ListIndexesAsync();
        if (!indexesResult.Result)
        {
            return new ErrorDataResult<bool>(indexesResult.Message, indexesResult.StatusCode);
        }
        
        return new SuccessDataResult<bool>(indexesResult.Data.Contains(indexName), "Index check completed");
    }
    
    public async Task<IResult> DeleteIndexAsync(string indexName)
    {
        return await _pineconeService.DeleteIndexAsync(indexName);
    }
    
    public async Task<IDataResult<int>> UpsertVectorsAsync(string indexName, List<VectorRecord> vectors)
    {
        // Convert VectorRecord to Pinecone format
        var pineconeVectors = vectors.Select(v => new PineconeVector
        {
            Id = v.Id,
            Values = v.Values,
            Metadata = v.Metadata
        }).ToList();
        
        var result = await _pineconeService.UpsertVectorsAsync(indexName, pineconeVectors);
        if (!result.Result)
        {
            return new ErrorDataResult<int>(result.Message, result.StatusCode);
        }
        
        return new SuccessDataResult<int>(pineconeVectors.Count, "Vectors upserted successfully");
    }
    
    public async Task<IDataResult<List<ScoredVectorRecord>>> QueryAsync(string indexName, float[] queryVector, int topK = 10, Dictionary<string, object>? filter = null)
    {
        var result = await _pineconeService.QueryAsync(indexName, queryVector, topK, filter);
        if (!result.Result)
        {
            return new ErrorDataResult<List<ScoredVectorRecord>>(result.Message, result.StatusCode);
        }
        
        // Convert Pinecone format to ScoredVectorRecord
        var scoredVectors = result.Data.Select(m => new ScoredVectorRecord
        {
            Id = m.Id,
            Values = m.Values,
            Metadata = m.Metadata,
            Score = m.Score
        }).ToList();
        
        return new SuccessDataResult<List<ScoredVectorRecord>>(scoredVectors, "Query completed successfully");
    }
    
    public async Task<IDataResult<int>> DeleteVectorsAsync(string indexName, List<string> ids)
    {
        var result = await _pineconeService.DeleteVectorsAsync(indexName, ids);
        if (!result.Result)
        {
            return new ErrorDataResult<int>(result.Message, result.StatusCode);
        }
        
        return new SuccessDataResult<int>(ids.Count, "Vectors deleted successfully");
    }
    
    public async Task<IDataResult<List<VectorRecord>>> FetchVectorsAsync(string indexName, List<string> ids)
    {
        var result = await _pineconeService.FetchVectorsAsync(indexName, ids);
        if (!result.Result)
        {
            return new ErrorDataResult<List<VectorRecord>>(result.Message, result.StatusCode);
        }
        
        // Convert Pinecone format to VectorRecord
        var vectors = result.Data.Select(v => new VectorRecord
        {
            Id = v.Id,
            Values = v.Values,
            Metadata = v.Metadata
        }).ToList();
        
        return new SuccessDataResult<List<VectorRecord>>(vectors, "Vectors fetched successfully");
    }
    
    public async Task<IDataResult<IndexStats>> GetIndexStatsAsync(string indexName)
    {
        var result = await _pineconeService.DescribeIndexStatsAsync(indexName);
        if (!result.Result)
        {
            return new ErrorDataResult<IndexStats>(result.Message, result.StatusCode);
        }
        
        var stats = new IndexStats
        {
            IndexName = indexName,
            Dimension = result.Data.Dimension,
            VectorCount = result.Data.TotalVectorCount,
            IndexSize = result.Data.IndexSize,
            CreatedAt = result.Data.CreatedAt
        };
        
        return new SuccessDataResult<IndexStats>(stats, "Index statistics retrieved successfully");
    }
}