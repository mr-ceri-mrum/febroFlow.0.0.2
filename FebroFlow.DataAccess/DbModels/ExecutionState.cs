using System.ComponentModel.DataAnnotations.Schema;

namespace FebroFlow.DataAccess.DbModels;

public class ExecutionState : BaseEntity
{
    // Reference to the flow
    public Guid FlowId { get; set; }
    [ForeignKey("FlowId")]
    public virtual Flow Flow { get; set; } = null!;
    
    // Execution context identifier (could be chat ID, session ID, etc.)
    public string ContextId { get; set; } = string.Empty;
    
    // State data
    public string StateData { get; set; } = "{}"; // JSON serialized state
    
    // Execution status
    public bool IsActive { get; set; } = true;
    public DateTime LastExecutionTime { get; set; } = DateTime.Now;
    
    // Last executed node
    public Guid? LastNodeId { get; set; }
    [ForeignKey("LastNodeId")]
    public virtual Node? LastNode { get; set; }
}
