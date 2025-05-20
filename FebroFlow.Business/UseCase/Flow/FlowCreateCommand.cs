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
    public CreateFlowRequest FlowDto { get; }

    public FlowCreateCommand(CreateFlowRequest flowDto)
    {
        FlowDto = flowDto;
    }
}

/// <summary>
/// Обработчик команды создания потока
/// </summary>
public class FlowCreateCommandHandler(
    IFlowDal flowDal,
    IAuthInformationRepository authInformationRepository,
    IMessagesRepository messagesRepository,
    IMapper mapper)
    : IRequestHandler<FlowCreateCommand, IDataResult<object>>
{
    public async Task<IDataResult<object>> Handle(FlowCreateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = authInformationRepository.GetUserId(); if (userId == Guid.Empty) return new ErrorDataResult<object>(messagesRepository.AccessDenied("User"), HttpStatusCode.Forbidden);
            userId = Guid.NewGuid();
            var flow = mapper.Map<DataAccess.DbModels.Flow>(request.FlowDto);
            if (userId != null) flow.CreatorId = userId.Value;
            
            await flowDal.AddAsync(flow);
            
            return new SuccessDataResult<object>(flow, messagesRepository.Created("Flow"));
        }  
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}