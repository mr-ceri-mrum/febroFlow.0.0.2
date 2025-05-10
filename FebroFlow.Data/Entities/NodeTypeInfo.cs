namespace FebroFlow.Data.Entities;

/// <summary>
/// Information about a node type
/// </summary>
public class NodeTypeInfo
{
    /// <summary>
    /// ID of the node type
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name of the node type
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the node
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the node type
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Input ports of the node
    /// </summary>
    public List<PortInfo> InputPorts { get; set; } = new List<PortInfo>();
    
    /// <summary>
    /// Output ports of the node
    /// </summary>
    public List<PortInfo> OutputPorts { get; set; } = new List<PortInfo>();
}

/// <summary>
/// Information about a port on a node
/// </summary>
public class PortInfo
{
    /// <summary>
    /// Name of the port
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Label to display for the port
    /// </summary>
    public string Label { get; set; } = string.Empty;
    
    /// <summary>
    /// Data type of the port
    /// </summary>
    public string Type { get; set; } = string.Empty;
}