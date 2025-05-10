using System.Net;
using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Node;

/// <summary>
/// Команда для создания соединения между узлами
/// </summary>
public class ConnectionCreateCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// DTO с данными для создания соединения
    /// </summary>
    public ConnectionCreateDto ConnectionDto { get; }

    public ConnectionCreateCommand(ConnectionCreateDto connectionDto)
    {
        ConnectionDto = connectionDto;
    }
}

/// <summary>
/// Обработчик команды создания соединения
/// </summary>
public class ConnectionCreateCommandHandler : IRequestHandler<ConnectionCreateCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionManager _connectionManager;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IMapper _mapper;

    public ConnectionCreateCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionManager connectionManager,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository,
        IMapper mapper)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionManager = connectionManager;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<object>> Handle(ConnectionCreateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            if (userId == Guid.Empty)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            }
            
            // Проверяем, существует ли поток
            var flow = await _flowDal.GetAsync(f => f.Id == request.ConnectionDto.FlowId);
            
            if (flow == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
            }
            
            // Проверяем права доступа к потоку
            if (flow.CreatorId != userId)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Проверяем, существуют ли узлы
            var sourceNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.SourceNodeId);
            var targetNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.TargetNodeId);
            
            if (sourceNode == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound("Source Node"), HttpStatusCode.NotFound);
            }
            
            if (targetNode == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound("Target Node"), HttpStatusCode.NotFound);
            }
            
            // Проверяем, принадлежат ли узлы этому потоку
            if (sourceNode.FlowId != request.ConnectionDto.FlowId || targetNode.FlowId != request.ConnectionDto.FlowId)
            {
                return new ErrorDataResult<object>("Узлы должны принадлежать к указанному потоку", HttpStatusCode.BadRequest);
            }
            
            // Проверяем, можно ли создать соединение между этими узлами
            var isValid = await _connectionManager.ValidateConnectionAsync(request.ConnectionDto.SourceNodeId, request.ConnectionDto.TargetNodeId);
            
            if (!isValid)
            {
                return new ErrorDataResult<object>("Невозможно создать соединение между указанными узлами", HttpStatusCode.BadRequest);
            }
            
            // Создаем соединение
            var connection = await _connectionManager.CreateConnectionAsync(
                request.ConnectionDto.SourceNodeId,
                request.ConnectionDto.TargetNodeId,
                request.ConnectionDto.FlowId);
            
            var connectionDto = _mapper.Map<ConnectionDto>(connection);
            
            return new SuccessDataResult<object>(connectionDto, _messagesRepository.Created("Connection"));
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}