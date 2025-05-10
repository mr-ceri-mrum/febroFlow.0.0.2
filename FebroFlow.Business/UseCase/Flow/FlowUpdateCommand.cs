using System.Net;
using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для обновления потока
/// </summary>
public class FlowUpdateCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// DTO с данными для обновления
    /// </summary>
    public FlowUpdateDto FlowDto { get; }

    public FlowUpdateCommand(FlowUpdateDto flowDto)
    {
        FlowDto = flowDto;
    }
}

/// <summary>
/// Обработчик команды обновления потока
/// </summary>
public class FlowUpdateCommandHandler : IRequestHandler<FlowUpdateCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IMapper _mapper;

    public FlowUpdateCommandHandler(
        IFlowDal flowDal,
        IAuthInformationRepository authInformationRepository,
        IMessagesRepository messagesRepository,
        IMapper mapper)
    {
        _flowDal = flowDal;
        _authInformationRepository = authInformationRepository;
        _messagesRepository = messagesRepository;
        _mapper = mapper;
    }

    public async Task<IDataResult<object>> Handle(FlowUpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            if (userId == Guid.Empty)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            }
            
            var flow = await _flowDal.GetAsync(f => f.Id == request.FlowDto.Id);
            
            if (flow == null)
            {
                return new ErrorDataResult<object>(_messagesRepository.NotFound(), HttpStatusCode.NotFound);
            }
            
            if (flow.CreatorId != userId)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("Flow"), HttpStatusCode.Forbidden);
            }
            
            // Обновляем только разрешенные поля
            flow.Name = request.FlowDto.Name;
            flow.Description = request.FlowDto.Description;
            flow.IsActive = request.FlowDto.IsActive;
            flow.Tags = request.FlowDto.Tags;
            flow.ModifiedDate = DateTime.UtcNow;
            
            // Сохраняем изменения
            await _flowDal.UpdateAsync(flow);
            
            // Маппим обратно на DTO
            var flowDto = _mapper.Map<FlowDto>(flow);
            
            return new SuccessDataResult<object>(flowDto, _messagesRepository.Edited("Flow"));
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}