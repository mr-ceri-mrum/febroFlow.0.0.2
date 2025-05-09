namespace FebroFlow.Data.Dtos.Flow;

public class FlowUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Tags { get; set; }
    public string? Settings { get; set; }
}