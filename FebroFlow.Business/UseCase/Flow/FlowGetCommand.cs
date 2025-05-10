using System.Net;
using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Dtos.Node;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для получения потока по ID
/// </summary>
public class FlowGetCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// Идентификатор потока
    /// </summary>
    public Guid FlowId { get; }
    
    /// <summary>
    /// Включать ли узлы потока
    /// </summary>
    public bool IncludeNodes { get; }
    
    /// <summary>
    /// Включать ли соединения потока
    /// </summary>
    public bool IncludeConnections { get; }

    public FlowGetCommand(Guid flowId, bool includeNodes = false, bool includeConnections = false)
    {
        FlowId = flowId;
        IncludeNodes = includeNodes;
        IncludeConnections = includeConnections;
    }
}

/// <summary>
/// Обработчик команды получения потока
/// </summary>
public class FlowGetCommandHandler : IRequestHandler<FlowGetCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IMapper _mapper;

    public FlowGetCommandHandler(
        IFlowDal flowDal,
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository,
        IMapper mapper)
    {
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<object>> Handle(FlowGetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            // Получаем поток
            var flow = await _flowDal.GetAsync(f => f.Id == request.FlowId);
            
            if (flow == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound(), HttpStatusCode.NotFound);
            }
            
            // Проверяем права доступа
            if (flow.CreatorId != userId && !flow.IsActive)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Создаем результат
            var flowDto = _mapper.Map<FlowDetailDto>(flow);
            
            // Добавляем узлы, если требуется
            if (request.IncludeNodes)
            {
                var nodes = await _nodeDal.GetAllAsync(n => n.FlowId == request.FlowId);
                flowDto.Nodes = _mapper.Map<List<NodeDto>>(nodes);
            }
            
            // Добавляем соединения, если требуется
            if (request.IncludeConnections)
            {
                var connections = await _connectionDal.GetAllAsync(c => c.FlowId == request.FlowId);
                flowDto.Connections = _mapper.Map<List<ConnectionDto>>(connections);
            }
            
            return new SuccessDataResult<object>(flowDto, "");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}