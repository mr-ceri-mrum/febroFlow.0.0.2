using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;
using Qdrant.Client;

namespace FebroFlow.Business.Services;

public interface IQdrantService
{
    /// <summary>
    /// Initialize the collection (create if it doesn't exist)
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>True if the collection was created, false if it already existed</returns>
    Task CreateCollectionAsync(string collectionName);

    /// <summary>
    /// Add or update a document in the collection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="document">Document to add or update</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> UpsertDocumentAsync(string id, string collectionName, float[] vector);
    
    /// <summary>
    /// Add or update multiple documents in the collection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="documents">Documents to add or update</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> UpsertDocumentsAsync(string collectionName, IEnumerable<DocumentModel> documents);
    
    /// <summary>
    /// Delete a document from the collection by ID
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="id">ID of document to delete</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> DeleteDocumentAsync(string collectionName, int id);

    /// <summary>
    /// Search for documents in the collection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <returns>Search response with results</returns>
    Task<List<IEnumerable<Value>>> SearchAsync(float[] vector, string collectionName);
    
    /// <summary>
    /// Get a document by ID
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="id">ID of document to retrieve</param>
    /// <returns>Document if found, null otherwise</returns>
    Task<DocumentModel?> GetDocumentAsync(string collectionName, int id);
    
    /// <summary>
    /// Get all documents in the collection
    /// </summary>
    /// <param name="collectionName">Name of the collection</param>
    /// <param name="limit">Maximum number of documents to return</param>
    /// <param name="offset">Number of documents to skip</param>
    /// <returns>List of documents</returns>
    Task<List<DocumentModel>> GetAllDocumentsAsync(string collectionName, int limit = 100, int offset = 0);
    
    /// <summary>
    /// Delete a collection
    /// </summary>
    /// <param name="collectionName">Name of the collection to delete</param>
    /// <returns>True if operation was successful</returns>
    Task<bool> DeleteCollectionAsync(string collectionName);
}


public class QdrantService(QdrantClient qdrantClient) : IQdrantService
{
    
    public async Task CreateCollectionAsync(string collectionName)
    {
        await qdrantClient.CreateCollectionAsync(collectionName);
    }
    
    public async Task<bool> UpsertDocumentAsync(string id, string collectionName, float[] vector)
    {
       /* var point = new PointStruct
        {
            Id = Guid.Parse(id),
            Vectors =  vector
        };
        
        await qdrantClient.UpsertAsync(collectionName, [point]);*/
        return true;
    }

    public Task<bool> UpsertDocumentsAsync(string collectionName, IEnumerable<DocumentModel> documents)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteDocumentAsync(string collectionName, int id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<IEnumerable<Value>>> SearchAsync(float[] vector, string collectionName)
    {
        var results = await qdrantClient.SearchAsync(collectionName, vector);
        var select = results.Select(x => x.Payload.Select(x => x.Value));
        
        return null;
    }
         
    public Task<DocumentModel?> GetDocumentAsync(string collectionName, int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<DocumentModel>> GetAllDocumentsAsync(string collectionName, int limit = 100, int offset = 0)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteCollectionAsync(string collectionName)
    {
        throw new NotImplementedException();
    }
}

public class DocumentModel
{ /// <summary>
    /// Unique identifier for the document
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The document's text content
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional metadata associated with the document
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Vector representation of the text (embedding)
    /// Only used internally - not included in serialization
    /// </summary>
    [JsonIgnore]
    public float[]? Vector { get; set; }
}

public class SearchRequest
{
    /// <summary>
    /// Query text to search for similar documents
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int Limit { get; set; } = 5;
    
    /// <summary>
    /// Minimum similarity score (0-1) to include in results
    /// </summary>
    public float[] ScoreThreshold { get; set; }
    
    /// <summary>
    /// Optional metadata filters
    /// </summary>
    public Dictionary<string, object>? Filters { get; set; }
}
