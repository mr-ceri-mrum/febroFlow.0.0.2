using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using System.Text.Json;

namespace FebroFlow.Business.Services;

public interface IExecutionStateManager
{
    /// <summary>
    /// Gets the current execution state for a flow and context
    /// </summary>
    Task<IDataResult<ExecutionState>> GetExecutionStateAsync(Guid flowId, string contextId);
    
    /// <summary>
    /// Updates the execution state with new data
    /// </summary>
    Task<IDataResult<ExecutionState>> UpdateExecutionStateAsync(Guid flowId, string contextId, object stateData, Guid? lastNodeId = null);
    
    /// <summary>
    /// Initializes a new execution state
    /// </summary>
    Task<IDataResult<ExecutionState>> InitializeExecutionStateAsync(Guid flowId, string contextId);
    
    /// <summary>
    /// Completes an execution state
    /// </summary>
    Task<IDataResult<ExecutionState>> CompleteExecutionStateAsync(Guid flowId, string contextId);
}

public class ExecutionStateManager : IExecutionStateManager
{
    private readonly IExecutionStateDal _executionStateDal;
    
    public ExecutionStateManager(IExecutionStateDal executionStateDal)
    {
        _executionStateDal = executionStateDal;
    }

    public async Task<IDataResult<ExecutionState>> GetExecutionStateAsync(Guid flowId, string contextId)
    {
        var state = await _executionStateDal.GetAsync(
            s => s.FlowId == flowId && s.ContextId == contextId && s.IsActive);
            
        if (state == null)
        {
            return new ErrorDataResult<ExecutionState>("Execution state not found", System.Net.HttpStatusCode.NotFound);
        }
        
        return new SuccessDataResult<ExecutionState>(state, "Execution state retrieved successfully");
    }

    public async Task<IDataResult<ExecutionState>> UpdateExecutionStateAsync(Guid flowId, string contextId, object stateData, Guid? lastNodeId = null)
    {
        var state = await _executionStateDal.GetAsync(
            s => s.FlowId == flowId && s.ContextId == contextId && s.IsActive);
            
        if (state == null)
        {
            // Create new state if it doesn't exist
            return await InitializeExecutionStateAsync(flowId, contextId);
        }
        
        // Update state
        state.StateData = JsonSerializer.Serialize(stateData);
        state.LastExecutionTime = DateTime.Now;
        
        if (lastNodeId.HasValue)
        {
            state.LastNodeId = lastNodeId.Value;
        }
        
        await _executionStateDal.UpdateAsync(state);
        
        return new SuccessDataResult<ExecutionState>(state, "Execution state updated successfully");
    }

    public async Task<IDataResult<ExecutionState>> InitializeExecutionStateAsync(Guid flowId, string contextId)
    {
        // Deactivate any existing active states for this context
        var existingStates = await _executionStateDal.GetAllAsync(
            s => s.FlowId == flowId && s.ContextId == contextId && s.IsActive);
            
        foreach (var state in existingStates)
        {
            state.IsActive = false;
            await _executionStateDal.UpdateAsync(state);
        }
        
        // Create new state
        var newState = new ExecutionState
        {
            FlowId = flowId,
            ContextId = contextId,
            StateData = "{}",
            IsActive = true,
            LastExecutionTime = DateTime.Now
        };
        
        await _executionStateDal.AddAsync(newState);
        
        return new SuccessDataResult<ExecutionState>(newState, "Execution state initialized successfully");
    }

    public async Task<IDataResult<ExecutionState>> CompleteExecutionStateAsync(Guid flowId, string contextId)
    {
        var state = await _executionStateDal.GetAsync(
            s => s.FlowId == flowId && s.ContextId == contextId && s.IsActive);
            
        if (state == null)
        {
            return new ErrorDataResult<ExecutionState>("Execution state not found", System.Net.HttpStatusCode.NotFound);
        }
        
        // Mark as inactive
        state.IsActive = false;
        await _executionStateDal.UpdateAsync(state);
        
        return new SuccessDataResult<ExecutionState>(state, "Execution state completed successfully");
    }
}