using febroFlow.DataAccess.DbModels;

namespace febroFlow.Business.Services;

/// <summary>
/// Interface for managing execution state of flows
/// </summary>
public interface IExecutionStateManager
{
    /// <summary>
    /// Initialize a new execution state for a flow
    /// </summary>
    /// <param name="flow">The flow to execute</param>
    /// <param name="initialData">Optional initial data for the flow</param>
    /// <returns>The execution ID</returns>
    Task<string> InitializeExecution(Flow flow, Dictionary<string, object>? initialData = null);
    
    /// <summary>
    /// Get the current state of an execution
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <returns>The current execution state</returns>
    Task<Dictionary<string, object>> GetExecutionState(string executionId);
    
    /// <summary>
    /// Update the state of an execution
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="state">The new state data</param>
    /// <returns>Task</returns>
    Task UpdateExecutionState(string executionId, Dictionary<string, object> state);
    
    /// <summary>
    /// Complete an execution and save its history
    /// </summary>
    /// <param name="executionId">The execution ID</param>
    /// <param name="success">Whether the execution was successful</param>
    /// <param name="result">The final result</param>
    /// <returns>Task</returns>
    Task CompleteExecution(string executionId, bool success, object? result = null);
}