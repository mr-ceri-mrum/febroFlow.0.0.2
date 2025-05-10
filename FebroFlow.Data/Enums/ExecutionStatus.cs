namespace FebroFlow.Data.Enums;

/// <summary>
/// Status of flow execution
/// </summary>
public enum ExecutionStatus
{
    /// <summary>
    /// Execution is initialized but not started
    /// </summary>
    Initialized = 0,
    
    /// <summary>
    /// Execution is in progress
    /// </summary>
    InProgress = 1,
    
    /// <summary>
    /// Execution is waiting for user input or external event
    /// </summary>
    Waiting = 2,
    
    /// <summary>
    /// Execution is paused by user
    /// </summary>
    Paused = 3,
    
    /// <summary>
    /// Execution completed successfully
    /// </summary>
    Completed = 4,
    
    /// <summary>
    /// Execution failed with an error
    /// </summary>
    Failed = 5,
    
    /// <summary>
    /// Execution was cancelled by user
    /// </summary>
    Cancelled = 6
}