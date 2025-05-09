using System.ComponentModel.DataAnnotations.Schema;

namespace FebroFlow.DataAccess.DbModels;

public class ChatMemory : BaseEntity
{
    // Chat identifier
    public string ChatId { get; set; } = string.Empty;
    
    // Message data
    public string Role { get; set; } = string.Empty; // 'user', 'ai', 'system'
    public string Content { get; set; } = string.Empty;
    
    // Timestamp for ordering
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    // Optional flow reference
    public Guid? FlowId { get; set; }
    [ForeignKey("FlowId")]
    public virtual Flow? Flow { get; set; }
}
