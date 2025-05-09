using FebroFlow.Data.Enums;

namespace FebroFlow.Data.Dtos.Node;

public class ConnectionCreateDto
{
    public required Guid SourceNodeId { get; set; }
    public required Guid TargetNodeId { get; set; }
    public ConnectionType Type { get; set; } = ConnectionType.Main;
    public int? SourceOutputIndex { get; set; }
    public int? TargetInputIndex { get; set; }
    public required Guid FlowId { get; set; }
}