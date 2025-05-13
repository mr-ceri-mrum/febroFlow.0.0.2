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
    
    Task<string> ExecuteFlowAsync(Guid flowId, string promt, string? chatId = null);
    Task<string> ExecuteFlowAsyncV2(Guid flowId, string promt, string? chatId);
}

public class FlowEngine: IFlowEngine
{
    public Task<Guid> ExecuteFlowAsync(Guid flowId, string contextId, object inputData)
    {
        throw new NotImplementedException();
    }

    public Task<ExecutionState> GetFlowExecutionStateAsync(Guid executionId)
    {
        throw new NotImplementedException();
    }

    public Task CancelFlowExecutionAsync(Guid executionId)
    {
        throw new NotImplementedException();
    }

    public async Task<string> ExecuteFlowAsync(Guid flowId, string promt, string? chatId)
    {
        throw new NotImplementedException();

    }

    public Task<string> ExecuteFlowAsyncV2(Guid flowId, string promt, string chatId)
    {
        throw new NotImplementedException();
    }
}