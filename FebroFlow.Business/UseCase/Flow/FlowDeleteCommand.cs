using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowDeleteCommand : IRequest<IDataResult<object>>
{
    public Guid Id { get; }

    public FlowDeleteCommand(Guid id)
    {
        Id = id;
    }
}

public class FlowDeleteCommandHandler : IRequestHandler<FlowDeleteCommand, IDataResult<object>>
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
    
    public async Task<IDataResult<object>> Handle(FlowDeleteCommand request, CancellationToken cancellationToken)
    {
        // Check if flow exists
        var flow = await _flowDal.GetAsync(x => x.Id == request.Id);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Mark flow as deleted
        await _flowDal.DeleteAsync(flow);
        
        // Also mark all associated nodes and connections as deleted
        await _nodeDal.DeleteAsync(x => x.FlowId == request.Id);
        await _connectionDal.DeleteAsync(x => x.FlowId == request.Id);
        
        return new SuccessDataResult<object>(true, _messagesRepository.Deleted());
    }
}