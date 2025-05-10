using System.ComponentModel.DataAnnotations.Schema;
using FebroFlow.Data.Enums;

namespace FebroFlow.DataAccess.DbModels;

public class ExecutionState : BaseEntity
{
    // Reference to the flow
    public Guid FlowId { get; set; }
    [ForeignKey("FlowId")]
    public virtual Flow Flow { get; set; } = null!;
    
    // Execution status
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Initialized;
    
    // Current node being executed
    public Guid? CurrentNodeId { get; set; }
    [ForeignKey("CurrentNodeId")]
    public virtual Node? CurrentNode { get; set; }
    
    // Context identifier (could be chat ID, session ID, etc.)
    public string ContextId { get; set; } = string.Empty;
    
    // Timestamps
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Data
    public string InputData { get; set; } = "{}";    // JSON serialized input
    public string OutputData { get; set; } = "{}";   // JSON serialized output
    public string CurrentData { get; set; } = "{}";  // JSON serialized current state
    
    // Execution path (JSON serialized list of nodes executed)
    public string ExecutionPath { get; set; } = "[]";
    
    // Error information
    public string? ErrorMessage { get; set; }
}