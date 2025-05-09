using System.Net;
using febroFlow.Core.Dtos.Flow;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Flow;

/// <summary>
/// Command to update an existing flow
/// </summary>
public class FlowUpdateCommand : IRequest<IResult>
{
    public FlowUpdateDto FlowDto { get; }

    public FlowUpdateCommand(FlowUpdateDto flowDto)
    {
        FlowDto = flowDto;
    }
}

/// <summary>
/// Handler for FlowUpdateCommand
/// </summary>
public class FlowUpdateCommandHandler : IRequestHandler<FlowUpdateCommand, IResult>
{
    private readonly IFlowDal _flowDal;
    private readonly IMessagesRepository _messagesRepository;

    public FlowUpdateCommandHandler(
        IFlowDal flowDal,
        IMessagesRepository messagesRepository)
    {
        _flowDal = flowDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to update a flow
    /// </summary>
    public async Task<IResult> Handle(FlowUpdateCommand request, CancellationToken cancellationToken)
    {
        // Get the flow by ID
        var flow = await _flowDal.GetAsync(f => f.Id == request.FlowDto.Id);
        if (flow == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Check if the name is being changed and if so, check uniqueness
        if (flow.Name != request.FlowDto.Name && 
            await _flowDal.AnyAsync(f => f.Name == request.FlowDto.Name && f.Id != request.FlowDto.Id))
        {
            return new ErrorResult(
                _messagesRepository.ShouldBeUnique("Flow name"),
                HttpStatusCode.BadRequest);
        }

        // Update flow properties
        flow.Name = request.FlowDto.Name;
        flow.Description = request.FlowDto.Description;
        flow.IsActive = request.FlowDto.IsActive;
        flow.ModifiedDate = DateTime.UtcNow;

        // Save to database
        await _flowDal.UpdateAsync(flow);

        // Return success
        return new SuccessResult(_messagesRepository.Edited("Flow"));
    }
}