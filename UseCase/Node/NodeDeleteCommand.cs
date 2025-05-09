using System.Net;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to delete a node by ID
/// </summary>
public class NodeDeleteCommand : IRequest<IResult>
{
    public Guid NodeId { get; }

    public NodeDeleteCommand(Guid nodeId)
    {
        NodeId = nodeId;
    }
}

/// <summary>
/// Handler for NodeDeleteCommand
/// </summary>
public class NodeDeleteCommandHandler : IRequestHandler<NodeDeleteCommand, IResult>
{
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public NodeDeleteCommandHandler(
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to delete a node and its connections
    /// </summary>
    public async Task<IResult> Handle(NodeDeleteCommand request, CancellationToken cancellationToken)
    {
        // Get the node
        var node = await _nodeDal.GetAsync(n => n.Id == request.NodeId);
        if (node == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Check if it's a start node
        if (node.NodeType?.Name == "Start")
        {
            return new ErrorResult(
                "Cannot delete the start node. A flow must have a start node.",
                HttpStatusCode.BadRequest);
        }

        // Get all connections related to this node
        var connections = await _connectionDal.GetAllAsync(
            c => c.SourceNodeId == request.NodeId || c.TargetNodeId == request.NodeId);
        
        // Delete all connections first
        await _connectionDal.DeleteRangeAsync(connections);
        
        // Delete the node
        await _nodeDal.DeleteAsync(node);
        
        // Return success
        return new SuccessResult(_messagesRepository.Deleted());
    }
}