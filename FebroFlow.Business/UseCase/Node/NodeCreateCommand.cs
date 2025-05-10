using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using MediatR;
using System.Net;
using FebroFlow.Core.Responses;
using febroFlow.DataAccess.DataAccess;

namespace FebroFlow.Business.UseCase.Node;

public class NodeCreateCommand : IRequest<IDataResult<object>>
{
    public NodeCreateDto Form { get; }

    public NodeCreateCommand(NodeCreateDto form)
    {
        Form = form;
    }
}

public class NodeCreateCommandHandler : IRequestHandler<NodeCreateCommand, IDataResult<object>>
{
    private readonly INodeDal _nodeDal;
    private readonly IFlowDal _flowDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public NodeCreateCommandHandler(
        INodeDal nodeDal,
        IFlowDal flowDal,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _flowDal = flowDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(NodeCreateCommand request, CancellationToken cancellationToken)
    {
        // Check if flow exists
        var flow = await _flowDal.GetAsync(x => x.Id == request.Form.FlowId);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Map DTO to entity
        var node = _mapper.Map<DataAccess.DbModels.Node>(request.Form);
        
        // Set creation data
        node.DataCreate = DateTime.Now;
        
        // Save to database
        await _nodeDal.AddAsync(node);
        
        return new SuccessDataResult<object>(node, _messagesRepository.Created("Node"));
    }
}