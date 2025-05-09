using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FebroFlow.DataAccess.DbModels;

namespace febroFlow.Services
{
    /// <summary>
    /// Interface for managing connections between nodes in a flow
    /// </summary>
    public interface IConnectionManager
    {
        /// <summary>
        /// Creates a new connection between nodes
        /// </summary>
        /// <param name="flowId">The ID of the flow</param>
        /// <param name="sourceNodeId">The ID of the source node</param>
        /// <param name="targetNodeId">The ID of the target node</param>
        /// <param name="outputPath">The output path from the source node</param>
        /// <param name="label">Optional label for the connection</param>
        /// <returns>The created connection</returns>
        Task<Connection> CreateConnectionAsync(Guid flowId, Guid sourceNodeId, Guid targetNodeId, string outputPath, string label = null);
        
        /// <summary>
        /// Updates an existing connection
        /// </summary>
        /// <param name="connectionId">The ID of the connection to update</param>
        /// <param name="sourceNodeId">The new source node ID</param>
        /// <param name="targetNodeId">The new target node ID</param>
        /// <param name="outputPath">The new output path</param>
        /// <param name="label">The new label</param>
        /// <returns>The updated connection</returns>
        Task<Connection> UpdateConnectionAsync(Guid connectionId, Guid sourceNodeId, Guid targetNodeId, string outputPath, string label = null);
        
        /// <summary>
        /// Deletes a connection
        /// </summary>
        /// <param name="connectionId">The ID of the connection to delete</param>
        /// <returns>True if the connection was successfully deleted</returns>
        Task<bool> DeleteConnectionAsync(Guid connectionId);
        
        /// <summary>
        /// Gets a connection by ID
        /// </summary>
        /// <param name="connectionId">The ID of the connection to get</param>
        /// <returns>The connection</returns>
        Task<Connection> GetConnectionAsync(Guid connectionId);
        
        /// <summary>
        /// Gets all connections for a flow
        /// </summary>
        /// <param name="flowId">The ID of the flow</param>
        /// <returns>The list of connections</returns>
        Task<List<Connection>> GetConnectionsForFlowAsync(Guid flowId);
        
        /// <summary>
        /// Gets all connections from a source node
        /// </summary>
        /// <param name="sourceNodeId">The ID of the source node</param>
        /// <returns>The list of connections</returns>
        Task<List<Connection>> GetConnectionsFromNodeAsync(Guid sourceNodeId);
        
        /// <summary>
        /// Gets all connections to a target node
        /// </summary>
        /// <param name="targetNodeId">The ID of the target node</param>
        /// <returns>The list of connections</returns>
        Task<List<Connection>> GetConnectionsToNodeAsync(Guid targetNodeId);
        
        /// <summary>
        /// Validates a connection to ensure it's valid within the flow
        /// </summary>
        /// <param name="flowId">The ID of the flow</param>
        /// <param name="sourceNodeId">The ID of the source node</param>
        /// <param name="targetNodeId">The ID of the target node</param>
        /// <returns>True if the connection is valid, otherwise false</returns>
        Task<bool> ValidateConnectionAsync(Guid flowId, Guid sourceNodeId, Guid targetNodeId);
    }
}