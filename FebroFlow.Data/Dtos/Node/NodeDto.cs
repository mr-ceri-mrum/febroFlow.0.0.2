using FebroFlow.Data.Enums;

namespace FebroFlow.Data.Dtos.Node;

public class NodeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public NodeType Type { get; set; }
    public string Parameters { get; set; } = "{}";
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public Guid FlowId { get; set; }
    public Guid? CredentialId { get; set; }
    public bool DisableRetryOnFail { get; set; }
    public bool AlwaysOutputData { get; set; }
    public DateTime DataCreate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}