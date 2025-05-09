using FebroFlow.Data.Enums;

namespace FebroFlow.Data.Dtos.Node;

public class ConnectionUpdateDto
{
    public Guid Id { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public ConnectionType Type { get; set; } = ConnectionType.Main;
    public int? SourceOutputIndex { get; set; }
    public int? TargetInputIndex { get; set; }
}