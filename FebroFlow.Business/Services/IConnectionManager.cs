using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Enums;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services;

public interface IConnectionManager
{
    /// <summary>
    /// Gets connections from a source node
    /// </summary>
    Task<IDataResult<List<Connection>>> GetConnectionsFromSourceAsync(Guid sourceNodeId);
    
    /// <summary>
    /// Gets connections to a target node
    /// </summary>
    Task<IDataResult<List<Connection>>> GetConnectionsToTargetAsync(Guid targetNodeId);
    
    /// <summary>
    /// Verifies if a connection is valid
    /// </summary>
    Task<IDataResult<bool>> ValidateConnectionAsync(Guid sourceNodeId, Guid targetNodeId, ConnectionType connectionType);
}

public class ConnectionManager : IConnectionManager
{
    private readonly IConnectionDal _connectionDal;
    private readonly INodeDal _nodeDal;
    
    public ConnectionManager(IConnectionDal connectionDal, INodeDal nodeDal)
    {
        _connectionDal = connectionDal;
        _nodeDal = nodeDal;
    }

    public async Task<IDataResult<List<Connection>>> GetConnectionsFromSourceAsync(Guid sourceNodeId)
    {
        var connections = await _connectionDal.GetAllAsync(c => c.SourceNodeId == sourceNodeId);
        return new SuccessDataResult<List<Connection>>(connections, "Connections retrieved successfully");
    }

    public async Task<IDataResult<List<Connection>>> GetConnectionsToTargetAsync(Guid targetNodeId)
    {
        var connections = await _connectionDal.GetAllAsync(c => c.TargetNodeId == targetNodeId);
        return new SuccessDataResult<List<Connection>>(connections, "Connections retrieved successfully");
    }

    public async Task<IDataResult<bool>> ValidateConnectionAsync(Guid sourceNodeId, Guid targetNodeId, ConnectionType connectionType)
    {
        // Verify nodes exist
        var sourceNode = await _nodeDal.GetAsync(n => n.Id == sourceNodeId);
        var targetNode = await _nodeDal.GetAsync(n => n.Id == targetNodeId);
        
        if (sourceNode == null || targetNode == null)
        {
            return new ErrorDataResult<bool>(false, "Source or target node not found", System.Net.HttpStatusCode.NotFound);
        }
        
        // Verify connection compatibility based on node types
        bool isCompatible = ValidateNodeTypeCompatibility(sourceNode.Type, targetNode.Type, connectionType);
        
        if (!isCompatible)
        {
            return new ErrorDataResult<bool>(false, "Connection types are not compatible", System.Net.HttpStatusCode.BadRequest);
        }
        
        return new SuccessDataResult<bool>(true, "Connection is valid");
    }
    
    private bool ValidateNodeTypeCompatibility(NodeType sourceType, NodeType targetType, ConnectionType connectionType)
    {
        // Implement logic to verify if source and target node types are compatible
        // with the given connection type
        
        // For example, only certain nodes can be connected via AI_Memory connections
        if (connectionType == ConnectionType.AI_Memory)
        {
            return sourceType == NodeType.MemoryManager || targetType == NodeType.MemoryManager;
        }
        
        // For AI_Retriever connections
        if (connectionType == ConnectionType.AI_Retriever)
        {
            return sourceType == NodeType.VectorStoreRetriever && 
                  (targetType == NodeType.ChainRetrievalQA || targetType == NodeType.SequentialThinking);
        }
        
        // For AI_VectorStore connections
        if (connectionType == ConnectionType.AI_VectorStore)
        {
            return sourceType == NodeType.PineconeVectorStore && targetType == NodeType.VectorStoreRetriever;
        }
        
        // For AI_Embedding connections
        if (connectionType == ConnectionType.AI_Embedding)
        {
            return sourceType == NodeType.EmbeddingsOpenAI && targetType == NodeType.PineconeVectorStore;
        }
        
        // For AI_LanguageModel connections
        if (connectionType == ConnectionType.AI_LanguageModel)
        {
            return sourceType == NodeType.OpenAIChatModel && 
                   (targetType == NodeType.ChainRetrievalQA || targetType == NodeType.SequentialThinking);
        }
        
        // For Conditional connections
        if (connectionType == ConnectionType.Conditional)
        {
            return sourceType == NodeType.IfCondition || sourceType == NodeType.Switch;
        }
        
        // Default Main connections are generally compatible between most nodes
        return true;
    }
}