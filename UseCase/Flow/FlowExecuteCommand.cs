using System.Net;
using febroFlow.Core.Exceptions;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DbModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to execute a flow with optional input data
/// </summary>
public class FlowExecuteCommand : IRequest<IDataResult<object>>
{
    public Guid FlowId { get; }
    public Dictionary<string, object> InputData { get; }

    public FlowExecuteCommand(Guid flowId, Dictionary<string, object>? inputData = null)
    {
        FlowId = flowId;
        InputData = inputData ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Handler for FlowExecuteCommand
/// </summary>
public class FlowExecuteCommandHandler : IRequestHandler<FlowExecuteCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IExecutionStateManager _executionStateManager;
    private readonly INodeFactory _nodeFactory;
    private readonly ILogger<FlowExecuteCommandHandler> _logger;

    public FlowExecuteCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository,
        IExecutionStateManager executionStateManager,
        INodeFactory nodeFactory,
        ILogger<FlowExecuteCommandHandler> logger)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
        _executionStateManager = executionStateManager;
        _nodeFactory = nodeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command to execute a flow
    /// </summary>
    public async Task<IDataResult<object>> Handle(FlowExecuteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the flow
            var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
            if (flow == null)
            {
                return new ErrorDataResult<object>(
                    _messagesRepository.NotFound(),
                    HttpStatusCode.NotFound);
            }

            // Check if flow is active
            if (!flow.IsActive)
            {
                return new ErrorDataResult<object>(
                    "Flow is not active",
                    HttpStatusCode.BadRequest);
            }

            // Get all nodes for this flow
            var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
            if (!nodes.Any())
            {
                return new ErrorDataResult<object>(
                    "Flow has no nodes",
                    HttpStatusCode.BadRequest);
            }

            // Get all connections for this flow
            var nodeIds = nodes.Select(n => n.Id).ToList();
            var connections = await _connectionDal.GetAllAsync(
                c => nodeIds.Contains(c.SourceNodeId) && nodeIds.Contains(c.TargetNodeId));

            // Find the start node
            var startNode = nodes.FirstOrDefault(n => n.NodeType.Name == "Start");
            if (startNode == null)
            {
                return new ErrorDataResult<object>(
                    "Flow has no start node",
                    HttpStatusCode.BadRequest);
            }

            // Initialize execution
            string executionId = await _executionStateManager.InitializeExecution(flow, request.InputData);
            
            // Start with the initial state
            var currentState = await _executionStateManager.GetExecutionState(executionId);
            
            // Execute the flow starting from the start node
            var result = await ExecuteNode(startNode, currentState, nodes, connections, executionId);
            
            // Complete the execution
            await _executionStateManager.CompleteExecution(executionId, true, result);
            
            return new SuccessDataResult<object>(result, "Flow executed successfully");
        }
        catch (ExecutionNotFoundException ex)
        {
            _logger.LogError(ex, "Execution not found during flow execution");
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing flow {FlowId}", request.FlowId);
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Recursively executes nodes in the flow
    /// </summary>
    private async Task<object> ExecuteNode(
        Node node, 
        Dictionary<string, object> state, 
        List<Node> allNodes, 
        List<Connection> connections,
        string executionId)
    {
        _logger.LogDebug("Executing node {NodeId} of type {NodeType}", node.Id, node.NodeType.Name);
        
        // Execute the current node
        var nodeInstance = await _nodeFactory.CreateNode(node);
        var result = await _nodeFactory.ExecuteNode(node, state, executionId);
        
        // Update the execution state
        await _executionStateManager.UpdateExecutionState(executionId, result);
        
        // Check if this is an end node
        if (node.NodeType.Name == "End")
        {
            return result;
        }
        
        // Find the next node(s) to execute
        var outgoingConnections = connections
            .Where(c => c.SourceNodeId == node.Id)
            .ToList();
        
        if (!outgoingConnections.Any())
        {
            _logger.LogWarning("Node {NodeId} has no outgoing connections", node.Id);
            return result;
        }
        
        // For conditional branches, evaluate the condition and take the appropriate path
        if (node.NodeType.Name == "Condition")
        {
            var condition = result.ContainsKey("condition") ? (bool)result["condition"] : false;
            
            var nextConnection = outgoingConnections
                .FirstOrDefault(c => c.Label == (condition ? "true" : "false"));
                
            if (nextConnection == null)
            {
                throw new InvalidOperationException($"No {(condition ? "true" : "false")} path found from condition node {node.Id}");
            }
            
            var nextNode = allNodes.FirstOrDefault(n => n.Id == nextConnection.TargetNodeId);
            if (nextNode == null)
            {
                throw new InvalidOperationException($"Target node {nextConnection.TargetNodeId} not found");
            }
            
            return await ExecuteNode(nextNode, result, allNodes, connections, executionId);
        }
        
        // For normal flow, just take the first connection
        var defaultNextConnection = outgoingConnections.First();
        var defaultNextNode = allNodes.FirstOrDefault(n => n.Id == defaultNextConnection.TargetNodeId);
        
        if (defaultNextNode == null)
        {
            throw new InvalidOperationException($"Target node {defaultNextConnection.TargetNodeId} not found");
        }
        
        return await ExecuteNode(defaultNextNode, result, allNodes, connections, executionId);
    }
}