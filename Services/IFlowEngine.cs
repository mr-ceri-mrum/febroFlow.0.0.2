using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FebroFlow.DataAccess.DbModels;

namespace febroFlow.Services
{
    /// <summary>
    /// Interface for the flow execution engine that manages the execution of flows
    /// </summary>
    public interface IFlowEngine
    {
        /// <summary>
        /// Starts the execution of a flow
        /// </summary>
        /// <param name="flowId">The ID of the flow to execute</param>
        /// <param name="input">The initial input data for the flow</param>
        /// <param name="conversationId">Optional conversation ID for tracking context</param>
        /// <returns>The execution state ID</returns>
        Task<Guid> StartFlowAsync(Guid flowId, Dictionary<string, object> input, string conversationId = null);
        
        /// <summary>
        /// Continues the execution of a flow from a specific execution state
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state to continue from</param>
        /// <param name="input">Additional input data</param>
        /// <returns>The updated execution state</returns>
        Task<ExecutionState> ContinueFlowAsync(Guid executionStateId, Dictionary<string, object> input);
        
        /// <summary>
        /// Checks if a flow is completed
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state to check</param>
        /// <returns>True if the flow is completed, otherwise false</returns>
        Task<bool> IsFlowCompletedAsync(Guid executionStateId);
        
        /// <summary>
        /// Gets the current output of a flow execution
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state</param>
        /// <returns>The current output data</returns>
        Task<Dictionary<string, object>> GetOutputAsync(Guid executionStateId);
        
        /// <summary>
        /// Pauses a running flow
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state to pause</param>
        /// <returns>True if the flow was successfully paused</returns>
        Task<bool> PauseFlowAsync(Guid executionStateId);
        
        /// <summary>
        /// Resumes a paused flow
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state to resume</param>
        /// <returns>True if the flow was successfully resumed</returns>
        Task<bool> ResumeFlowAsync(Guid executionStateId);
        
        /// <summary>
        /// Cancels a running flow
        /// </summary>
        /// <param name="executionStateId">The ID of the execution state to cancel</param>
        /// <returns>True if the flow was successfully cancelled</returns>
        Task<bool> CancelFlowAsync(Guid executionStateId);
    }
}