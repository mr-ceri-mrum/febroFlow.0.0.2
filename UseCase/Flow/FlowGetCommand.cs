using System.Net;
using febroFlow.Core.Dtos.Flow;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to get a flow by ID
/// </summary>
public class FlowGetCommand : IRequest<IDataResult<FlowDetailDto>>
{
    public Guid FlowId { get; }
    public bool IncludeNodes { get; }
    public bool IncludeConnections { get; }

    public FlowGetCommand(Guid flowId, bool includeNodes = true, bool includeConnections = true)
    {
        FlowId = flowId;
        IncludeNodes = includeNodes;
        IncludeConnections = includeConnections;
    }
}

/// <summary>
/// Handler for FlowGetCommand
/// </summary>
public class FlowGetCommandHandler : IRequestHandler<FlowGetCommand, IDataResult<FlowDetailDto>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public FlowGetCommandHandler(
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
    /// Handles the command to get a flow by ID
    /// </summary>
    public async Task<IDataResult<FlowDetailDto>> Handle(FlowGetCommand request, CancellationToken cancellationToken)
    {
        // Get the flow
        var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
        if (flow == null)
        {
            return new ErrorDataResult<FlowDetailDto>(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Create the base DTO
        var flowDto = new FlowDetailDto
        {
            Id = flow.Id,
            Name = flow.Name,
            Description = flow.Description,
            IsActive = flow.IsActive,
            CreatedAt = flow.DataCreate,
            ModifiedAt = flow.ModifiedDate,
            Nodes = new List<NodeDto>(),
            Connections = new List<ConnectionDto>()
        };

        // Include nodes if requested
        if (request.IncludeNodes)
        {
            var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
            flowDto.Nodes = nodes.Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                NodeTypeId = n.NodeTypeId,
                NodeTypeName = n.NodeType?.Name,
                Position = n.Position,
                Config = n.Config,
                CreatedAt = n.DataCreate,
                ModifiedAt = n.ModifiedDate
            }).ToList();
        }

        // Include connections if requested and nodes were included
        if (request.IncludeConnections && request.IncludeNodes)
        {
            var nodeIds = flowDto.Nodes.Select(n => n.Id).ToList();
            var connections = await _connectionDal.GetAllAsync(
                c => nodeIds.Contains(c.SourceNodeId) && nodeIds.Contains(c.TargetNodeId));
                
            flowDto.Connections = connections.Select(c => new ConnectionDto
            {
                Id = c.Id,
                SourceNodeId = c.SourceNodeId,
                TargetNodeId = c.TargetNodeId,
                Label = c.Label,
                CreatedAt = c.DataCreate,
                ModifiedAt = c.ModifiedDate
            }).ToList();
        }

        return new SuccessDataResult<FlowDetailDto>(flowDto, "Flow retrieved successfully");
    }
}