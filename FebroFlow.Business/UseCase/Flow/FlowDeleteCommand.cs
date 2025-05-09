using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для удаления потока
/// </summary>
public class FlowDeleteCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// Идентификатор потока для удаления
    /// </summary>
    public Guid FlowId { get; }

    public FlowDeleteCommand(Guid flowId)
    {
        FlowId = flowId;
    }
}

/// <summary>
/// Обработчик команды удаления потока
/// </summary>
public class FlowDeleteCommandHandler : IRequestHandler<FlowDeleteCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;

    public FlowDeleteCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
    }

    public async Task<IDataResult<object>> Handle(FlowDeleteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            if (userId == Guid.Empty)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            }
            
            var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
            
            if (flow == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound(), HttpStatusCode.NotFound);
            }
            
            if (flow.UserId != userId)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Удаляем все соединения для этого потока
            var connections = await _connectionDal.GetAllAsync(c => c.FlowId == request.FlowId);
            foreach (var connection in connections)
            {
                await _connectionDal.DeleteAsync(connection);
            }
            
            // Удаляем все узлы для этого потока
            var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
            foreach (var node in nodes)
            {
                await _nodeDal.DeleteAsync(node);
            }
            
            // Удаляем сам поток
            await _flowDal.DeleteAsync(flow);
            
            return new SuccessDataResult<object>(_messagesRepository.Deleted());
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}