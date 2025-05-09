using System.Net;
using System.Text.Json;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DbModels;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to create a new node in a flow
/// </summary>
public class NodeCreateCommand : IRequest<IDataResult<Guid>>
{
    public NodeCreateDto NodeDto { get; }

    public NodeCreateCommand(NodeCreateDto nodeDto)
    {
        NodeDto = nodeDto;
    }
}

/// <summary>
/// Handler for NodeCreateCommand
/// </summary>
public class NodeCreateCommandHandler : IRequestHandler<NodeCreateCommand, IDataResult<Guid>>
{
    private readonly INodeDal _nodeDal;
    private readonly IFlowDal _flowDal;
    private readonly INodeTypeDal _nodeTypeDal;
    private readonly IMessagesRepository _messagesRepository;

    public NodeCreateCommandHandler(
        INodeDal nodeDal,
        IFlowDal flowDal,
        INodeTypeDal nodeTypeDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _flowDal = flowDal;
        _nodeTypeDal = nodeTypeDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to create a new node
    /// </summary>
    public async Task<IDataResult<Guid>> Handle(NodeCreateCommand request, CancellationToken cancellationToken)
    {
        // Validate flow exists
        var flow = await _flowDal.GetAsync(f => f.Id == request.NodeDto.FlowId);
        if (flow == null)
        {
            return new ErrorDataResult<Guid>(
                _messagesRepository.NotFound("Flow"),
                HttpStatusCode.BadRequest);
        }

        // Validate node type exists
        var nodeType = await _nodeTypeDal.GetAsync(nt => nt.Id == request.NodeDto.NodeTypeId);
        if (nodeType == null)
        {
            return new ErrorDataResult<Guid>(
                _messagesRepository.NotFound("Node type"),
                HttpStatusCode.BadRequest);
        }

        // Validate start node uniqueness if this is a start node
        if (nodeType.Name == "Start" && await _nodeDal.AnyAsync(n => 
            n.FlowId == request.NodeDto.FlowId && 
            n.NodeType.Name == "Start"))
        {
            return new ErrorDataResult<Guid>(
                "A flow can only have one start node",
                HttpStatusCode.BadRequest);
        }

        // Create node entity
        var node = new Node
        {
            Id = Guid.NewGuid(),
            Name = request.NodeDto.Name,
            FlowId = request.NodeDto.FlowId,
            NodeTypeId = request.NodeDto.NodeTypeId,
            Position = request.NodeDto.Position ?? new Position { X = 0, Y = 0 },
            Config = request.NodeDto.Config ?? "{}"
        };

        // Save to database
        await _nodeDal.AddAsync(node);

        // Return success result with the new node ID
        return new SuccessDataResult<Guid>(node.Id, _messagesRepository.Created("Node"));
    }
}