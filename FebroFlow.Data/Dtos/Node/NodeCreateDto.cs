using FebroFlow.Data.Enums;

namespace FebroFlow.Data.Dtos.Node;

public class NodeCreateDto
{
    public required string Name { get; set; }
    public required NodeType Type { get; set; }
    public string Parameters { get; set; } = "{}";
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public required Guid FlowId { get; set; }
    public Guid? CredentialId { get; set; }
    public bool DisableRetryOnFail { get; set; } = false;
    public bool AlwaysOutputData { get; set; } = true;
}