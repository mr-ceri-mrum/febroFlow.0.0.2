using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Enums;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для выполнения потока
/// </summary>
public class FlowExecuteCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// Идентификатор потока для выполнения
    /// </summary>
    public Guid FlowId { get; }
    
    /// <summary>
    /// Входные данные для выполнения
    /// </summary>
    public object InputData { get; }

    public FlowExecuteCommand(Guid flowId, object inputData)
    {
        FlowId = flowId;
        InputData = inputData;
    }
}

/// <summary>
/// Обработчик команды выполнения потока
/// </summary>
public class FlowExecuteCommandHandler : IRequestHandler<FlowExecuteCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IExecutionStateManager _executionStateManager;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;

    public FlowExecuteCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IExecutionStateManager executionStateManager,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _executionStateManager = executionStateManager;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
    }

    public async Task<IDataResult<object>> Handle(FlowExecuteCommand request, CancellationToken cancellationToken)
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
            
            // Проверяем права доступа к потоку
            if (flow.CreatorId != userId && !flow.IsActive)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Получаем узлы потока
            var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
            
            if (!nodes.Any())
            {
                return new ErrorDataResult<object>("Поток не содержит узлов", HttpStatusCode.BadRequest);
            }
            
            // Получаем соединения между узлами
            var connections = await _connectionDal.GetAllAsync(c => c.FlowId == request.FlowId);
            
            // Инициализируем состояние выполнения
            var executionId = await _executionStateManager.InitializeExecutionStateAsync(request.FlowId, request.InputData);
            
            // Находим начальный узел (тот, у которого нет входящих соединений)
            var startNode = nodes.FirstOrDefault(n => 
                !connections.Any(c => c.TargetNodeId == n.Id));
                
            if (startNode == null)
            {
                return new ErrorDataResult<object>("Не найден начальный узел потока", HttpStatusCode.BadRequest);
            }
            
            // Обновляем состояние выполнения - начинаем с начального узла
            var executionState = await _executionStateManager.UpdateExecutionStateAsync(
                executionId, 
                startNode.Id, 
                ExecutionStatus.InProgress, 
                request.InputData);
            
            // Возвращаем идентификатор выполнения
            var result = new FlowExecutionResultDto
            {
                ExecutionId = executionId,
                Status = ExecutionStatus.InProgress,
                StartTime = DateTime.UtcNow
            };
            
            return new SuccessDataResult<object>(result, "Выполнение потока запущено");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}

public class FlowExecutionResultDto
{
    public Guid ExecutionId { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTime StartTime { get; set; }
};