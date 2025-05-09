using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowCreateCommand : IRequest<IDataResult<object>>
{
    public FlowCreateDto Form { get; }

    public FlowCreateCommand(FlowCreateDto form)
    {
        Form = form;
    }
}

public class FlowCreateCommandHandler : IRequestHandler<FlowCreateCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public FlowCreateCommandHandler(IFlowDal flowDal, IMapper mapper, IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(FlowCreateCommand request, CancellationToken cancellationToken)
    {
        // Map DTO to entity
        var flow = _mapper.Map<DataAccess.DbModels.Flow>(request.Form);
        
        // Set creation data
        flow.DataCreate = DateTime.Now;
        
        // Save to database
        await _flowDal.AddAsync(flow);
        
        return new SuccessDataResult<object>(flow, _messagesRepository.Created("Flow"));
    }
}