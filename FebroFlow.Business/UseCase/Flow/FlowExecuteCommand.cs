using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowExecuteCommand : IRequest<IDataResult<object>>
{
    public Guid FlowId { get; }
    public string ContextId { get; }
    public string? TriggerData { get; }

    public FlowExecuteCommand(Guid flowId, string contextId, string? triggerData = null)
    {
        FlowId = flowId;
        ContextId = contextId;
        TriggerData = triggerData;
    }
}

public class FlowExecuteCommandHandler : IRequestHandler<FlowExecuteCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IFlowEngine _flowEngine;
    private readonly IMessagesRepository _messagesRepository;
    
    public FlowExecuteCommandHandler(
        IFlowDal flowDal,
        IFlowEngine flowEngine,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _flowEngine = flowEngine;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(FlowExecuteCommand request, CancellationToken cancellationToken)
    {
        // Check if flow exists
        var flow = await _flowDal.GetAsync(x => x.Id == request.FlowId);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Check if flow is active
        if (!flow.IsActive)
        {
            return new ErrorDataResult<object>("Flow is not active", HttpStatusCode.BadRequest);
        }
        
        // Execute flow
        var result = await _flowEngine.ExecuteFlowAsync(request.FlowId, request.ContextId, request.TriggerData);
        
        return result;
    }
}