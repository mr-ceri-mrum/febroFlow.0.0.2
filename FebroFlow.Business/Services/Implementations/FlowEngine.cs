using System.Text.Json;
using FebroFlow.Data.Enums;
using FebroFlow.DataAccess.DbModels;
using febroFlow.DataAccess.DataAccess;
using Microsoft.Extensions.Logging;

namespace FebroFlow.Business.Services.Implementations;

/// <summary>
/// Implementation of flow engine for executing flows
/// </summary>
public class FlowEngine : IFlowEngine
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IExecutionStateManager _executionStateManager;
    private readonly INodeFactory _nodeFactory;
    private readonly ILogger<FlowEngine> _logger;
    
    public FlowEngine(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IExecutionStateManager executionStateManager,
        INodeFactory nodeFactory,
        ILogger<FlowEngine> logger)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _executionStateManager = executionStateManager;
        _nodeFactory = nodeFactory;
        _logger = logger;
    }
    
    /// <summary>
    /// Execute a flow with given input data
    /// </summary>
    public async Task<Guid> ExecuteFlowAsync(Guid flowId, string contextId, object inputData)
    {
        _logger.LogInformation("Starting execution of flow {FlowId} with context {ContextId}", flowId, contextId);
        
        // Validate flow exists and is active
        var flow = await _flowDal.GetAsync(f => f.Id == flowId);
        if (flow == null)
        {
            throw new ArgumentException($"Flow with ID {flowId} not found");
        }
        
        if (!flow.IsActive)
        {
            throw new InvalidOperationException($"Flow {flowId} is not active");
        }
        
        // Initialize execution state
        var executionId = await _executionStateManager.InitializeExecutionStateAsync(flowId, inputData);
        
        // Get all nodes for the flow
        var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == flowId);
        if (!nodes.Any())
        {
            _logger.LogWarning("Flow {FlowId} has no nodes", flowId);
            
            // Complete execution with empty result
            await _executionStateManager.CompleteExecutionAsync(executionId, ExecutionStatus.Completed, new { });
            return executionId;
        }
        
        // Get all connections
        var connections = await _connectionDal.GetAllAsync(c => c.FlowId == flowId);
        
        // Find start node (usually a trigger node)
        var startNode = nodes.FirstOrDefault(n => !connections.Any(c => c.TargetNodeId == n.Id));
        if (startNode == null)
        {
            _logger.LogWarning("Flow {FlowId} has no start node", flowId);
            
            // Try to find any node as a fallback
            startNode = nodes.FirstOrDefault();
            
            if (startNode == null)
            {
                await _executionStateManager.CompleteExecutionAsync(executionId, ExecutionStatus.Failed, 
                    new { error = "No nodes found in flow" });
                return executionId;
            }
        }
        
        try
        {
            // Start execution with the first node
            var executionState = await _executionStateManager.UpdateExecutionStateAsync(
                executionId, 
                startNode.Id, 
                ExecutionStatus.InProgress, 
                inputData);
            
            // Process the flow
            var result = await ProcessNodeAsync(executionId, startNode, nodes, connections, 
                JsonSerializer.Deserialize<Dictionary<string, object>>(executionState.CurrentData) ?? new Dictionary<string, object>());
            
            // Complete execution
            await _executionStateManager.CompleteExecutionAsync(executionId, ExecutionStatus.Completed, result);
            
            _logger.LogInformation("Flow {FlowId} execution completed successfully", flowId);
            
            return executionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing flow {FlowId}", flowId);
            
            // Mark execution as failed
            await _executionStateManager.CompleteExecutionAsync(executionId, ExecutionStatus.Failed, 
                new { error = ex.Message });
                
            throw;
        }
    }
    
    /// <summary>
    /// Process a node in the flow
    /// </summary>
    private async Task<object> ProcessNodeAsync(Guid executionId, Node node, List<Node> nodes, List<Connection> connections, Dictionary<string, object> data)
    {
        _logger.LogDebug("Processing node {NodeName} ({NodeId})", node.Name, node.Id);
        
        // Process this node using the NodeFactory
        // In a real implementation, this would call the appropriate node handler
        // For now, we'll just pass the data through as a simulation
        
        // Get output connections from this node
        var outputConnections = connections.Where(c => c.SourceNodeId == node.Id).ToList();
        if (!outputConnections.Any())
        {
            // This is an end node, return the current data
            return data;
        }
        
        // For now, just take the first connection
        var nextConnection = outputConnections.First();
        var nextNode = nodes.FirstOrDefault(n => n.Id == nextConnection.TargetNodeId);
        if (nextNode == null)
        {
            throw new InvalidOperationException($"Target node {nextConnection.TargetNodeId} not found");
        }
        
        // Update execution state for the next node
        await _executionStateManager.UpdateExecutionStateAsync(
            executionId,
            nextNode.Id,
            ExecutionStatus.InProgress,
            data);
            
        // Process the next node
        return await ProcessNodeAsync(executionId, nextNode, nodes, connections, data);
    }
    
    /// <summary>
    /// Get the state of a flow execution
    /// </summary>
    public async Task<ExecutionState> GetFlowExecutionStateAsync(Guid executionId)
    {
        return await _executionStateManager.GetExecutionStateAsync(executionId);
    }
    
    /// <summary>
    /// Cancel a flow execution
    /// </summary>
    public async Task CancelFlowExecutionAsync(Guid executionId)
    {
        var executionState = await _executionStateManager.GetExecutionStateAsync(executionId);
        
        // Only cancel if not already completed or failed
        if (executionState.Status != ExecutionStatus.Completed && 
            executionState.Status != ExecutionStatus.Failed &&
            executionState.Status != ExecutionStatus.Cancelled)
        {
            await _executionStateManager.CompleteExecutionAsync(
                executionId, 
                ExecutionStatus.Cancelled, 
                new { cancelled = true });
        }
    }
}