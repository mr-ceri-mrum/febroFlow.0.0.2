using febroFlow.DataAccess.DbModels;
using febroFlow.DataAccess.Nodes;

namespace febroFlow.Business.Services;

/// <summary>
/// Factory interface for creating and executing flow nodes
/// </summary>
public interface INodeFactory
{
    /// <summary>
    /// Get the list of all available node types
    /// </summary>
    /// <returns>List of node types</returns>
    Task<List<NodeType>> GetNodeTypes();
    
    /// <summary>
    /// Create a node instance based on the node model
    /// </summary>
    /// <param name="node">The node model</param>
    /// <returns>The node instance</returns>
    Task<INode> CreateNode(Node node);
    
    /// <summary>
    /// Execute a node with the given data
    /// </summary>
    /// <param name="node">The node to execute</param>
    /// <param name="data">Input data for the node</param>
    /// <param name="executionId">The current execution ID</param>
    /// <returns>Result of the node execution</returns>
    Task<Dictionary<string, object>> ExecuteNode(Node node, Dictionary<string, object> data, string executionId);
    
    /// <summary>
    /// Get the available node type categories
    /// </summary>
    /// <returns>List of node categories</returns>
    Task<List<string>> GetNodeCategories();
    
    /// <summary>
    /// Get a list of node types by category
    /// </summary>
    /// <param name="category">The category</param>
    /// <returns>List of node types in the category</returns>
    Task<List<NodeType>> GetNodeTypesByCategory(string category);
}