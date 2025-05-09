using System.ComponentModel.DataAnnotations.Schema;

namespace FebroFlow.DataAccess.DbModels;

public class Flow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid CreatorId { get; set; }
    
    // Navigation properties
    public virtual ICollection<Node> Nodes { get; set; } = new List<Node>();
    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();
    
    // Additional metadata
    public string? Tags { get; set; }
    public string? Settings { get; set; } // JSON serialized settings
}
