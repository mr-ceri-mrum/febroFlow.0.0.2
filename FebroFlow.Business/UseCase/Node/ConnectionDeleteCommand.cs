using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Node;

/// <summary>
/// Команда для удаления соединения
/// </summary>
public class ConnectionDeleteCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// Идентификатор соединения
    /// </summary>
    public Guid ConnectionId { get; }

    public ConnectionDeleteCommand(Guid connectionId)
    {
        ConnectionId = connectionId;
    }
}

/// <summary>
/// Обработчик команды удаления соединения
/// </summary>
public class ConnectionDeleteCommandHandler : IRequestHandler<ConnectionDeleteCommand, IDataResult<object>>
{
    private readonly IConnectionDal _connectionDal;
    private readonly IFlowDal _flowDal;
    private readonly IConnectionManager _connectionManager;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;

    public ConnectionDeleteCommandHandler(
        IConnectionDal connectionDal,
        IFlowDal flowDal,
        IConnectionManager connectionManager,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _flowDal = flowDal;
        _connectionManager = connectionManager;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
    }

    public async Task<IDataResult<object>> Handle(ConnectionDeleteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            if (userId == Guid.Empty)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            }
            
            // Получаем соединение
            var connection = await _connectionDal.GetAsync(c => c.Id == request.ConnectionId);
            
            if (connection == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound(), HttpStatusCode.NotFound);
            }
            
            // Проверяем, имеет ли пользователь доступ к потоку, которому принадлежит соединение
            var flow = await _flowDal.GetAsync(f => f.Id == connection.FlowId);
            
            if (flow == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
            }
            
            if (flow.CreatorId != userId)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Удаляем соединение
            var result = await _connectionManager.DeleteConnectionAsync(request.ConnectionId);
            
            if (!result)
            {
                return new ErrorDataResult<object>("Не удалось удалить соединение", HttpStatusCode.InternalServerError);
            }
            
            return new SuccessDataResult<object>(_messagesRepository.Deleted());
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}