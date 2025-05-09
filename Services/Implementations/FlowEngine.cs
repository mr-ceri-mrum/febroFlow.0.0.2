using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using febroFlow.DataAccess.DataAccess;
using febroFlow.Services;
using FebroFlow.DataAccess.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace febroFlow.Services.Implementations
{
    /// <summary>
    /// Implementation of the flow execution engine that manages the execution of flows
    /// </summary>
    public class FlowEngine : IFlowEngine
    {
        private readonly IFlowDal _flowDal;
        private readonly INodeDal _nodeDal;
        private readonly IConnectionDal _connectionDal;
        private readonly IExecutionStateDal _executionStateDal;
        private readonly IExecutionStateManager _executionStateManager;
        private readonly INodeFactory _nodeFactory;
        private readonly ILogger<FlowEngine> _logger;
        
        /// <summary>
        /// Initializes a new instance of the FlowEngine class
        /// </summary>
        public FlowEngine(
            IFlowDal flowDal,
            INodeDal nodeDal,
            IConnectionDal connectionDal,
            IExecutionStateDal executionStateDal,
            IExecutionStateManager executionStateManager,
            INodeFactory nodeFactory,
            ILogger<FlowEngine> logger)
        {
            _flowDal = flowDal;
            _nodeDal = nodeDal;
            _connectionDal = connectionDal;
            _executionStateDal = executionStateDal;
            _executionStateManager = executionStateManager;
            _nodeFactory = nodeFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Guid> StartFlowAsync(Guid flowId, Dictionary<string, object> input, string conversationId = null)
        {
            try
            {
                _logger.LogInformation("Starting flow {FlowId} with input {Input}", flowId, JsonSerializer.Serialize(input));
                
                // Get the flow
                var flow = await _flowDal.GetAsync(f => f.Id == flowId);
                if (flow == null)
                {
                    throw new ArgumentException($"Flow with ID {flowId} not found.");
                }

                // Get the start node (typically a trigger node)
                var startNode = await _nodeDal.GetAsync(n => n.FlowId == flowId && n.IsStartNode);
                if (startNode == null)
                {
                    throw new InvalidOperationException($"No start node found for flow {flowId}.");
                }

                // Create a new execution state
                var executionState = new ExecutionState
                {
                    FlowId = flowId,
                    CurrentNodeId = startNode.Id,
                    Status = "Running",
                    Input = JsonSerializer.Serialize(input),
                    Output = "{}",
                    Logs = "[]",
                    Variables = "{}",
                    ConversationId = conversationId
                };
                
                await _executionStateDal.AddAsync(executionState);
                
                // Start executing the flow
                await ExecuteNodeAsync(executionState, startNode, input);
                
                return executionState.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting flow {FlowId}", flowId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<ExecutionState> ContinueFlowAsync(Guid executionStateId, Dictionary<string, object> input)
        {
            try
            {
                _logger.LogInformation("Continuing flow execution {ExecutionStateId} with input {Input}", executionStateId, JsonSerializer.Serialize(input));
                
                // Get the execution state
                var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
                if (executionState == null)
                {
                    throw new ArgumentException($"Execution state with ID {executionStateId} not found.");
                }
                
                if (executionState.Status != "Waiting" && executionState.Status != "Paused")
                {
                    throw new InvalidOperationException($"Cannot continue execution state {executionStateId} because it is in {executionState.Status} status.");
                }
                
                // Get the current node
                var currentNode = await _nodeDal.GetAsync(n => n.Id == executionState.CurrentNodeId);
                if (currentNode == null)
                {
                    throw new InvalidOperationException($"Current node {executionState.CurrentNodeId} for execution state {executionStateId} not found.");
                }
                
                // Update the execution state
                executionState.Status = "Running";
                var existingInput = JsonSerializer.Deserialize<Dictionary<string, object>>(executionState.Input);
                foreach (var kvp in input)
                {
                    existingInput[kvp.Key] = kvp.Value;
                }
                executionState.Input = JsonSerializer.Serialize(existingInput);
                
                await _executionStateDal.UpdateAsync(executionState);
                
                // Continue executing the flow
                await ExecuteNodeAsync(executionState, currentNode, existingInput);
                
                // Refresh execution state after execution
                return await _executionStateDal.GetAsync(es => es.Id == executionStateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error continuing flow execution {ExecutionStateId}", executionStateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsFlowCompletedAsync(Guid executionStateId)
        {
            var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
            return executionState != null && executionState.Status == "Completed";
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, object>> GetOutputAsync(Guid executionStateId)
        {
            var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
            if (executionState == null)
            {
                throw new ArgumentException($"Execution state with ID {executionStateId} not found.");
            }
            
            return JsonSerializer.Deserialize<Dictionary<string, object>>(executionState.Output);
        }

        /// <inheritdoc/>
        public async Task<bool> PauseFlowAsync(Guid executionStateId)
        {
            try
            {
                var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
                if (executionState == null)
                {
                    throw new ArgumentException($"Execution state with ID {executionStateId} not found.");
                }
                
                if (executionState.Status != "Running")
                {
                    _logger.LogWarning("Cannot pause flow execution {ExecutionStateId} because it is in {Status} status.", executionStateId, executionState.Status);
                    return false;
                }
                
                executionState.Status = "Paused";
                await _executionStateDal.UpdateAsync(executionState);
                
                _logger.LogInformation("Flow execution {ExecutionStateId} paused.", executionStateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing flow execution {ExecutionStateId}", executionStateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ResumeFlowAsync(Guid executionStateId)
        {
            try
            {
                var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
                if (executionState == null)
                {
                    throw new ArgumentException($"Execution state with ID {executionStateId} not found.");
                }
                
                if (executionState.Status != "Paused")
                {
                    _logger.LogWarning("Cannot resume flow execution {ExecutionStateId} because it is in {Status} status.", executionStateId, executionState.Status);
                    return false;
                }
                
                // Get the current node
                var currentNode = await _nodeDal.GetAsync(n => n.Id == executionState.CurrentNodeId);
                if (currentNode == null)
                {
                    throw new InvalidOperationException($"Current node {executionState.CurrentNodeId} for execution state {executionStateId} not found.");
                }
                
                // Update the execution state
                executionState.Status = "Running";
                await _executionStateDal.UpdateAsync(executionState);
                
                // Continue executing the flow
                var input = JsonSerializer.Deserialize<Dictionary<string, object>>(executionState.Input);
                await ExecuteNodeAsync(executionState, currentNode, input);
                
                _logger.LogInformation("Flow execution {ExecutionStateId} resumed.", executionStateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming flow execution {ExecutionStateId}", executionStateId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CancelFlowAsync(Guid executionStateId)
        {
            try
            {
                var executionState = await _executionStateDal.GetAsync(es => es.Id == executionStateId);
                if (executionState == null)
                {
                    throw new ArgumentException($"Execution state with ID {executionStateId} not found.");
                }
                
                if (executionState.Status == "Completed" || executionState.Status == "Cancelled" || executionState.Status == "Failed")
                {
                    _logger.LogWarning("Cannot cancel flow execution {ExecutionStateId} because it is in {Status} status.", executionStateId, executionState.Status);
                    return false;
                }
                
                executionState.Status = "Cancelled";
                await _executionStateDal.UpdateAsync(executionState);
                
                _logger.LogInformation("Flow execution {ExecutionStateId} cancelled.", executionStateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling flow execution {ExecutionStateId}", executionStateId);
                throw;
            }
        }
        
        /// <summary>
        /// Executes a node and continues flow execution
        /// </summary>
        private async Task ExecuteNodeAsync(ExecutionState executionState, Node node, Dictionary<string, object> input)
        {
            try
            {
                // Create node instance
                var nodeInstance = _nodeFactory.CreateNode(node.Type);
                if (nodeInstance == null)
                {
                    throw new InvalidOperationException($"Failed to create node instance for type {node.Type}.");
                }
                
                // Set node properties
                var nodeConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(node.Configuration);
                nodeInstance.Initialize(node.Id, nodeConfig);
                
                // Execute the node
                var result = await nodeInstance.ExecuteAsync(input, _executionStateManager);
                
                // Update execution state with output
                await _executionStateManager.UpdateOutputAsync(executionState.Id, result.Output);
                
                // Check if we need to wait for user interaction
                if (result.IsWaiting)
                {
                    executionState.Status = "Waiting";
                    await _executionStateDal.UpdateAsync(executionState);
                    return;
                }
                
                // Find the next node to execute
                await FindAndExecuteNextNodeAsync(executionState, node, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing node {NodeId} for execution state {ExecutionStateId}", node.Id, executionState.Id);
                
                // Mark execution state as failed
                executionState.Status = "Failed";
                executionState.ErrorMessage = ex.Message;
                await _executionStateDal.UpdateAsync(executionState);
                
                throw;
            }
        }
        
        /// <summary>
        /// Finds and executes the next node based on the output path
        /// </summary>
        private async Task FindAndExecuteNextNodeAsync(ExecutionState executionState, Node currentNode, NodeExecutionResult result)
        {
            // Get the connections from the current node
            var connections = await _connectionDal.GetAllAsQueryable(filter: c => 
                c.FlowId == executionState.FlowId && 
                c.SourceNodeId == currentNode.Id && 
                c.OutputPath == result.OutputPath);
            
            var connectionsList = await connections.ToListAsync();
            
            if (!connectionsList.Any())
            {
                // If no connections match the output path, check if there's a default connection
                connections = await _connectionDal.GetAllAsQueryable(filter: c => 
                    c.FlowId == executionState.FlowId && 
                    c.SourceNodeId == currentNode.Id && 
                    c.OutputPath == "default");
                
                connectionsList = await connections.ToListAsync();
            }
            
            if (!connectionsList.Any())
            {
                // If no connections, mark execution as completed
                executionState.Status = "Completed";
                await _executionStateDal.UpdateAsync(executionState);
                return;
            }
            
            // Get the next node to execute
            var nextConnection = connectionsList.First();
            var nextNode = await _nodeDal.GetAsync(n => n.Id == nextConnection.TargetNodeId);
            
            if (nextNode == null)
            {
                throw new InvalidOperationException($"Next node {nextConnection.TargetNodeId} not found.");
            }
            
            // Update execution state
            executionState.CurrentNodeId = nextNode.Id;
            await _executionStateDal.UpdateAsync(executionState);
            
            // Execute the next node
            await ExecuteNodeAsync(executionState, nextNode, result.Output);
        }
    }
    
    /// <summary>
    /// Result of node execution
    /// </summary>
    public class NodeExecutionResult
    {
        /// <summary>
        /// Output data from the node
        /// </summary>
        public Dictionary<string, object> Output { get; set; }
        
        /// <summary>
        /// Output path to follow
        /// </summary>
        public string OutputPath { get; set; }
        
        /// <summary>
        /// Whether the node execution is waiting for user interaction
        /// </summary>
        public bool IsWaiting { get; set; }
    }
}