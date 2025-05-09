using febroFlow.Core.Dtos.Node;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DbModels;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to get all available node types
/// </summary>
public class NodeGetTypesCommand : IRequest<IDataResult<List<NodeTypeDto>>>
{
    public string? Category { get; }

    public NodeGetTypesCommand(string? category = null)
    {
        Category = category;
    }
}

/// <summary>
/// Handler for NodeGetTypesCommand
/// </summary>
public class NodeGetTypesCommandHandler : IRequestHandler<NodeGetTypesCommand, IDataResult<List<NodeTypeDto>>>
{
    private readonly INodeFactory _nodeFactory;

    public NodeGetTypesCommandHandler(INodeFactory nodeFactory)
    {
        _nodeFactory = nodeFactory;
    }

    /// <summary>
    /// Handles the command to get all available node types
    /// </summary>
    public async Task<IDataResult<List<NodeTypeDto>>> Handle(NodeGetTypesCommand request, CancellationToken cancellationToken)
    {
        // Get all node types
        List<NodeType> nodeTypes;
        
        if (!string.IsNullOrEmpty(request.Category))
        {
            // Filter by category if provided
            nodeTypes = await _nodeFactory.GetNodeTypesByCategory(request.Category);
        }
        else
        {
            // Get all node types
            nodeTypes = await _nodeFactory.GetNodeTypes();
        }

        // Map to DTOs
        var nodeDtos = nodeTypes.Select(nt => new NodeTypeDto
        {
            Id = nt.Id,
            Name = nt.Name,
            Category = nt.Category,
            Description = nt.Description,
            Icon = nt.Icon,
            Color = nt.Color,
            InputPorts = nt.InputPorts?.Select(p => new PortDto
            {
                Name = p.Name,
                Label = p.Label,
                Type = p.Type
            }).ToList() ?? new List<PortDto>(),
            OutputPorts = nt.OutputPorts?.Select(p => new PortDto
            {
                Name = p.Name,
                Label = p.Label,
                Type = p.Type
            }).ToList() ?? new List<PortDto>(),
            ConfigSchema = nt.ConfigSchema
        }).ToList();

        return new SuccessDataResult<List<NodeTypeDto>>(nodeDtos, "Node types retrieved successfully");
    }
}