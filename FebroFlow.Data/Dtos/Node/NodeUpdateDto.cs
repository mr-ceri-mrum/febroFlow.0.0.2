using FebroFlow.Data.Enums;

namespace FebroFlow.Data.Dtos.Node;

public class NodeUpdateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Parameters { get; set; } = "{}";
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public Guid? CredentialId { get; set; }
    public bool DisableRetryOnFail { get; set; }
    public bool AlwaysOutputData { get; set; }
}