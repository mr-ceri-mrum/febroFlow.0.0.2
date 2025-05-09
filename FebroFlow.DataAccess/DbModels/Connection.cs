using System.ComponentModel.DataAnnotations.Schema;
using FebroFlow.Data.Enums;

namespace FebroFlow.DataAccess.DbModels;

public class Connection : BaseEntity
{
    // Source node
    public Guid SourceNodeId { get; set; }
    [ForeignKey("SourceNodeId")]
    public virtual Node SourceNode { get; set; } = null!;
    
    // Target node
    public Guid TargetNodeId { get; set; }
    [ForeignKey("TargetNodeId")]
    public virtual Node TargetNode { get; set; } = null!;
    
    // Connection type
    public ConnectionType Type { get; set; } = ConnectionType.Main;
    
    // Indices if needed for multiple outputs from a node
    public int? SourceOutputIndex { get; set; }
    public int? TargetInputIndex { get; set; }
    
    // Flow relationship
    public Guid FlowId { get; set; }
    [ForeignKey("FlowId")]
    public virtual Flow Flow { get; set; } = null!;
}
