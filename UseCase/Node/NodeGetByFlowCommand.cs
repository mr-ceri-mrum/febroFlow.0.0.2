using System.Net;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to get all nodes for a specific flow
/// </summary>
public class NodeGetByFlowCommand : IRequest<IDataResult<List<NodeDto>>>
{
    public Guid FlowId { get; }

    public NodeGetByFlowCommand(Guid flowId)
    {
        FlowId = flowId;
    }
}

/// <summary>
/// Handler for NodeGetByFlowCommand
/// </summary>
public class NodeGetByFlowCommandHandler : IRequestHandler<NodeGetByFlowCommand, IDataResult<List<NodeDto>>>
{
    private readonly INodeDal _nodeDal;
    private readonly IFlowDal _flowDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public NodeGetByFlowCommandHandler(
        INodeDal nodeDal,
        IFlowDal flowDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _flowDal = flowDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to get all nodes for a flow
    /// </summary>
    public async Task<IDataResult<List<NodeDto>>> Handle(NodeGetByFlowCommand request, CancellationToken cancellationToken)
    {
        // Validate flow exists
        var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
        if (flow == null)
        {
            return new ErrorDataResult<List<NodeDto>>(
                _messagesRepository.NotFound("Flow"),
                HttpStatusCode.NotFound);
        }

        // Get all nodes for this flow
        var nodes = await _nodeDal.GetAllAsQueryable()
            .Where(n => n.FlowId == request.FlowId)
            .Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                NodeTypeId = n.NodeTypeId,
                NodeTypeName = n.NodeType.Name,
                Position = n.Position,
                Config = n.Config,
                CreatedAt = n.DataCreate,
                ModifiedAt = n.ModifiedDate
            })
            .ToListAsync(cancellationToken);

        return new SuccessDataResult<List<NodeDto>>(nodes, "Nodes retrieved successfully");
    }
}