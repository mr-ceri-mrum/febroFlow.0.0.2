using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FebroFlow.Business.UseCase.Node;

public class NodeGetByFlowCommand : IRequest<IDataResult<object>>
{
    public Guid FlowId { get; }

    public NodeGetByFlowCommand(Guid flowId)
    {
        FlowId = flowId;
    }
}

public class NodeGetByFlowCommandHandler : IRequestHandler<NodeGetByFlowCommand, IDataResult<object>>
{
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public NodeGetByFlowCommandHandler(
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(NodeGetByFlowCommand request, CancellationToken cancellationToken)
    {
        // Retrieve nodes for the flow
        var nodes = await _nodeDal.GetAllAsync(x => x.FlowId == request.FlowId);
        var connections = await _connectionDal.GetAllAsync(x => x.FlowId == request.FlowId);
        
        // Map to DTOs
        var nodeDtos = _mapper.Map<List<NodeDto>>(nodes);
        var connectionDtos = _mapper.Map<List<ConnectionDto>>(connections);
        
        var result = new
        {
            Nodes = nodeDtos,
            Connections = connectionDtos
        };
        
        return new SuccessDataResult<object>(result, "Flow nodes retrieved successfully");
    }
}