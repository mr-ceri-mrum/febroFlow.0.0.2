using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace febroFlow.Services
{
    /// <summary>
    /// Interface for interacting with Pinecone vector database service
    /// </summary>
    public interface IPineconeService
    {
        /// <summary>
        /// Inserts or updates a vector in the Pinecone index
        /// </summary>
        /// <param name="vectorId">The unique ID for the vector</param>
        /// <param name="vector">The vector representation (embeddings)</param>
        /// <param name="metadata">Additional metadata to store with the vector</param>
        /// <returns>True if the operation was successful</returns>
        Task<bool> UpsertVectorAsync(string vectorId, float[] vector, Dictionary<string, object> metadata);
        
        /// <summary>
        /// Queries vectors that are similar to the provided vector
        /// </summary>
        /// <param name="vector">The query vector</param>
        /// <param name="topK">Number of most similar vectors to return</param>
        /// <returns>List of search results ordered by similarity</returns>
        Task<List<SearchResult>> QueryAsync(float[] vector, int topK = 10);
        
        /// <summary>
        /// Deletes a vector from the Pinecone index
        /// </summary>
        /// <param name="vectorId">The ID of the vector to delete</param>
        /// <returns>True if the deletion was successful</returns>
        Task<bool> DeleteVectorAsync(string vectorId);
        
        /// <summary>
        /// Fetches a vector and its metadata by ID
        /// </summary>
        /// <param name="vectorId">The ID of the vector to fetch</param>
        /// <returns>The vector and associated metadata</returns>
        Task<Dictionary<string, object>> FetchVectorAsync(string vectorId);
    }
    
    /// <summary>
    /// Represents a search result from a vector similarity search
    /// </summary>
    public class SearchResult
    {
        /// <summary>
        /// The ID of the vector
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// The similarity score (typically a value between 0 and 1)
        /// </summary>
        public float Score { get; set; }
        
        /// <summary>
        /// Metadata associated with the vector
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }
    }
}