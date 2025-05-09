using System.Net;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to delete a flow by ID
/// </summary>
public class FlowDeleteCommand : IRequest<IResult>
{
    public Guid FlowId { get; }

    public FlowDeleteCommand(Guid flowId)
    {
        FlowId = flowId;
    }
}

/// <summary>
/// Handler for FlowDeleteCommand
/// </summary>
public class FlowDeleteCommandHandler : IRequestHandler<FlowDeleteCommand, IResult>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public FlowDeleteCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to delete a flow and all its related nodes and connections
    /// </summary>
    public async Task<IResult> Handle(FlowDeleteCommand request, CancellationToken cancellationToken)
    {
        // Get the flow by ID
        var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
        if (flow == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Get all nodes in this flow
        var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
        
        // Get all connections related to this flow's nodes
        var nodeIds = nodes.Select(n => n.Id).ToList();
        var connections = await _connectionDal.GetAllAsync(
            c => nodeIds.Contains(c.SourceNodeId) || nodeIds.Contains(c.TargetNodeId));
        
        // Delete all connections first
        await _connectionDal.DeleteRangeAsync(connections);
        
        // Delete all nodes
        await _nodeDal.DeleteRangeAsync(nodes);
        
        // Delete the flow
        await _flowDal.DeleteAsync(flow);
        
        // Return success
        return new SuccessResult(_messagesRepository.Deleted());
    }
}