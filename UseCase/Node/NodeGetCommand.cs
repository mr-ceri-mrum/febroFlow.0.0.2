using System.Net;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to get a node by ID
/// </summary>
public class NodeGetCommand : IRequest<IDataResult<NodeDetailDto>>
{
    public Guid NodeId { get; }

    public NodeGetCommand(Guid nodeId)
    {
        NodeId = nodeId;
    }
}

/// <summary>
/// Handler for NodeGetCommand
/// </summary>
public class NodeGetCommandHandler : IRequestHandler<NodeGetCommand, IDataResult<NodeDetailDto>>
{
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public NodeGetCommandHandler(
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to get a node by ID
    /// </summary>
    public async Task<IDataResult<NodeDetailDto>> Handle(NodeGetCommand request, CancellationToken cancellationToken)
    {
        // Get the node with its node type
        var node = await _nodeDal.GetAsync(n => n.Id == request.NodeId,
            n => n.NodeType);
            
        if (node == null)
        {
            return new ErrorDataResult<NodeDetailDto>(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Get connections related to this node
        var incomingConnections = await _connectionDal.GetAllAsync(c => c.TargetNodeId == request.NodeId);
        var outgoingConnections = await _connectionDal.GetAllAsync(c => c.SourceNodeId == request.NodeId);

        // Create DTO
        var nodeDto = new NodeDetailDto
        {
            Id = node.Id,
            Name = node.Name,
            NodeTypeId = node.NodeTypeId,
            NodeTypeName = node.NodeType?.Name,
            Position = node.Position,
            Config = node.Config,
            FlowId = node.FlowId,
            CreatedAt = node.DataCreate,
            ModifiedAt = node.ModifiedDate,
            IncomingConnections = incomingConnections.Select(c => new ConnectionDto
            {
                Id = c.Id,
                SourceNodeId = c.SourceNodeId,
                TargetNodeId = c.TargetNodeId,
                Label = c.Label,
                CreatedAt = c.DataCreate,
                ModifiedAt = c.ModifiedDate
            }).ToList(),
            OutgoingConnections = outgoingConnections.Select(c => new ConnectionDto
            {
                Id = c.Id,
                SourceNodeId = c.SourceNodeId,
                TargetNodeId = c.TargetNodeId,
                Label = c.Label,
                CreatedAt = c.DataCreate,
                ModifiedAt = c.ModifiedDate
            }).ToList()
        };

        return new SuccessDataResult<NodeDetailDto>(nodeDto, "Node retrieved successfully");
    }
}