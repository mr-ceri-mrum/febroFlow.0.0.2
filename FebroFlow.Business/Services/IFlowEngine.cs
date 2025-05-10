using FebroFlow.DataAccess.DbModels;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for flow execution engine
/// </summary>
public interface IFlowEngine
{
    /// <summary>
    /// Execute a flow with given input data
    /// </summary>
    /// <param name="flowId">ID of the flow to execute</param>
    /// <param name="contextId">Context identifier for execution (e.g., chat ID, session ID)</param>
    /// <param name="inputData">Input data for the flow</param>
    /// <returns>Execution ID for tracking the flow execution</returns>
    Task<Guid> ExecuteFlowAsync(Guid flowId, string contextId, object inputData);
    
    /// <summary>
    /// Get the state of a flow execution
    /// </summary>
    /// <param name="executionId">ID of the execution to retrieve</param>
    /// <returns>Current execution state</returns>
    Task<ExecutionState> GetFlowExecutionStateAsync(Guid executionId);
    
    /// <summary>
    /// Cancel a flow execution
    /// </summary>
    /// <param name="executionId">ID of the execution to cancel</param>
    Task CancelFlowExecutionAsync(Guid executionId);
}