using System.Net;
using febroFlow.Core.Dtos.Flow;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DbModels;
using MediatR;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to create a new flow
/// </summary>
public class FlowCreateCommand : IRequest<IDataResult<Guid>>
{
    public FlowCreateDto FlowDto { get; }

    public FlowCreateCommand(FlowCreateDto flowDto)
    {
        FlowDto = flowDto;
    }
}

/// <summary>
/// Handler for FlowCreateCommand
/// </summary>
public class FlowCreateCommandHandler : IRequestHandler<FlowCreateCommand, IDataResult<Guid>>
{
    private readonly IFlowDal _flowDal;
    private readonly IMessagesRepository _messagesRepository;

    public FlowCreateCommandHandler(
        IFlowDal flowDal,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to create a new flow
    /// </summary>
    public async Task<IDataResult<Guid>> Handle(FlowCreateCommand request, CancellationToken cancellationToken)
    {
        // Check if a flow with the same name already exists
        if (await _flowDal.AnyAsync(f => f.Name == request.FlowDto.Name))
        {
            return new ErrorDataResult<Guid>(
                _messagesRepository.ShouldBeUnique("Flow name"),
                HttpStatusCode.BadRequest);
        }

        // Create new flow entity
        var flow = new DataAccess.DbModels.Flow
        {
            Id = Guid.NewGuid(),
            Name = request.FlowDto.Name,
            Description = request.FlowDto.Description,
            IsActive = request.FlowDto.IsActive
        };

        // Save to database
        await _flowDal.AddAsync(flow);

        // Return success result with the new flow ID
        return new SuccessDataResult<Guid>(flow.Id, _messagesRepository.Created("Flow"));
    }
}