using System.Net;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to update an existing node
/// </summary>
public class NodeUpdateCommand : IRequest<IResult>
{
    public NodeUpdateDto NodeDto { get; }

    public NodeUpdateCommand(NodeUpdateDto nodeDto)
    {
        NodeDto = nodeDto;
    }
}

/// <summary>
/// Handler for NodeUpdateCommand
/// </summary>
public class NodeUpdateCommandHandler : IRequestHandler<NodeUpdateCommand, IResult>
{
    private readonly INodeDal _nodeDal;
    private readonly INodeTypeDal _nodeTypeDal;
    private readonly IMessagesRepository _messagesRepository;

    public NodeUpdateCommandHandler(
        INodeDal nodeDal,
        INodeTypeDal nodeTypeDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _nodeTypeDal = nodeTypeDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to update a node
    /// </summary>
    public async Task<IResult> Handle(NodeUpdateCommand request, CancellationToken cancellationToken)
    {
        // Get the node
        var node = await _nodeDal.GetAsync(n => n.Id == request.NodeDto.Id);
        if (node == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Check node type if it's changed
        if (node.NodeTypeId != request.NodeDto.NodeTypeId)
        {
            // Validate node type exists
            var nodeType = await _nodeTypeDal.GetAsync(nt => nt.Id == request.NodeDto.NodeTypeId);
            if (nodeType == null)
            {
                return new ErrorResult(
                    _messagesRepository.NotFound("Node type"),
                    HttpStatusCode.BadRequest);
            }
            
            // If this is a start node, check uniqueness
            if (nodeType.Name == "Start" && await _nodeDal.AnyAsync(n => 
                n.Id != request.NodeDto.Id &&
                n.FlowId == node.FlowId && 
                n.NodeType.Name == "Start"))
            {
                return new ErrorResult(
                    "A flow can only have one start node",
                    HttpStatusCode.BadRequest);
            }
        }

        // Update node properties
        node.Name = request.NodeDto.Name;
        node.NodeTypeId = request.NodeDto.NodeTypeId;
        node.Position = request.NodeDto.Position ?? node.Position;
        node.Config = request.NodeDto.Config ?? node.Config;
        node.ModifiedDate = DateTime.UtcNow;

        // Save to database
        await _nodeDal.UpdateAsync(node);

        // Return success
        return new SuccessResult(_messagesRepository.Edited("Node"));
    }
}