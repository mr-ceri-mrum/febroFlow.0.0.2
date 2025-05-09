using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowUpdateCommand : IRequest<IDataResult<object>>
{
    public FlowUpdateDto Form { get; }

    public FlowUpdateCommand(FlowUpdateDto form)
    {
        Form = form;
    }
}

public class FlowUpdateCommandHandler : IRequestHandler<FlowUpdateCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public FlowUpdateCommandHandler(IFlowDal flowDal, IMapper mapper, IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(FlowUpdateCommand request, CancellationToken cancellationToken)
    {
        // Check if flow exists
        var flow = await _flowDal.GetAsync(x => x.Id == request.Form.Id);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Update properties
        flow.Name = request.Form.Name;
        flow.Description = request.Form.Description;
        flow.IsActive = request.Form.IsActive;
        flow.Tags = request.Form.Tags;
        flow.Settings = request.Form.Settings;
        flow.ModifiedDate = DateTime.Now;
        
        // Save changes
        await _flowDal.UpdateAsync(flow);
        
        return new SuccessDataResult<object>(flow, _messagesRepository.Edited("Flow"));
    }
}