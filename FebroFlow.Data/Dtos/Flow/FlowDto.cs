namespace FebroFlow.Data.Dtos.Flow;

public class FlowDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid CreatorId { get; set; }
    public string? Tags { get; set; }
    public string? Settings { get; set; }
    public DateTime DataCreate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    
    // Navigation collections for API responses
    public List<NodeDto>? Nodes { get; set; }
    public List<ConnectionDto>? Connections { get; set; }
}