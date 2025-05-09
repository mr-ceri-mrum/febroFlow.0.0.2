using System.ComponentModel.DataAnnotations.Schema;
using FebroFlow.Data.Enums;

namespace FebroFlow.DataAccess.DbModels;

public class Node : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public string Parameters { get; set; } = "{}"; // JSON serialized parameters
    
    // Position in flow editor
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    
    // Flow relationship
    public Guid FlowId { get; set; }
    [ForeignKey("FlowId")]
    public virtual Flow Flow { get; set; } = null!;
    
    // Credentials reference if needed
    public Guid? CredentialId { get; set; }
    
    // Execution & state data
    public bool DisableRetryOnFail { get; set; } = false;
    public bool AlwaysOutputData { get; set; } = true;
}
