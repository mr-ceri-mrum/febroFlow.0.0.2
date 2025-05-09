using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowGetCommand : IRequest<IDataResult<object>>
{
    public Guid Id { get; }

    public FlowGetCommand(Guid id)
    {
        Id = id;
    }
}

public class FlowGetCommandHandler : IRequestHandler<FlowGetCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public FlowGetCommandHandler(
        IFlowDal flowDal, 
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMapper mapper, 
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(FlowGetCommand request, CancellationToken cancellationToken)
    {
        // Retrieve flow with its nodes and connections
        var flow = await _flowDal.GetAsync(x => x.Id == request.Id);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Map to DTO
        var flowDto = _mapper.Map<FlowDto>(flow);
        
        // Retrieve related nodes and connections separately for better performance
        var nodes = await _nodeDal.GetAllAsync(x => x.FlowId == request.Id);
        var connections = await _connectionDal.GetAllAsync(x => x.FlowId == request.Id);
        
        flowDto.Nodes = _mapper.Map<List<Data.Dtos.Node.NodeDto>>(nodes);
        flowDto.Connections = _mapper.Map<List<Data.Dtos.Node.ConnectionDto>>(connections);
        
        return new SuccessDataResult<object>(flowDto, "Flow retrieved successfully");
    }
}