namespace FebroFlow.DataAccess.DbModels;

public class Credential : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "telegram", "openai", "pinecone"
    
    // Encrypted data
    public string Data { get; set; } = string.Empty; // Encrypted JSON with credentials
    
    // Owner
    public Guid UserId { get; set; }
}
