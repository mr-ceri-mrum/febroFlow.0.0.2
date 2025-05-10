namespace FebroFlow.DataAccess.DbModels;

/// <summary>
/// Represents a node in the execution path history
/// </summary>
public class ExecutionPathItem
{
    /// <summary>
    /// ID of the node that was executed
    /// </summary>
    public Guid NodeId { get; set; }
    
    /// <summary>
    /// Timestamp when the node was executed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Data at this execution step (JSON serialized)
    /// </summary>
    public string Data { get; set; } = "{}";
}