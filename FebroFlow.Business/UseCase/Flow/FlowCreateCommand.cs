using System.Net;
using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Entities;
using febroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

/// <summary>
/// Команда для создания нового потока
/// </summary>
public class FlowCreateCommand : IRequest<IDataResult<object>>
{
    /// <summary>
    /// DTO для создания потока
    /// </summary>
    public FlowCreateDto FlowDto { get; }

    public FlowCreateCommand(FlowCreateDto flowDto)
    {
        FlowDto = flowDto;
    }
}

/// <summary>
/// Обработчик команды создания потока
/// </summary>
public class FlowCreateCommandHandler : IRequestHandler<FlowCreateCommand, IDataResult<object>>
{
    private readonly IFlowDal _flowDal;
    private readonly IAuthInformationRepository _authInformationRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IMapper _mapper;
    
    public FlowCreateCommandHandler(
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

    public async Task<IDataResult<object>> Handle(FlowCreateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _authInformationRepository.GetUserId();
            
            if (userId == Guid.Empty)
            {
                return new ErrorDataResult<object>(_messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            }
            
            var flow = _mapper.Map<DataAccess.DbModels.Flow>(request.FlowDto);
            flow.CreatorId = userId.Id;
            
            await _flowDal.AddAsync(flow);
            
            return new SuccessDataResult<object>(flow, _messagesRepository.Created("Flow"));
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}