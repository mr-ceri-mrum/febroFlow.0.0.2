namespace FebroFlow.Data.Dtos.Flow;

public class FlowCreateDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string? Settings { get; set; }
}