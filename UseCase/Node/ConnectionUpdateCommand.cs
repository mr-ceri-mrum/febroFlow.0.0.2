using System.Net;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to update an existing connection
/// </summary>
public class ConnectionUpdateCommand : IRequest<IResult>
{
    public ConnectionUpdateDto ConnectionDto { get; }

    public ConnectionUpdateCommand(ConnectionUpdateDto connectionDto)
    {
        ConnectionDto = connectionDto;
    }
}

/// <summary>
/// Handler for ConnectionUpdateCommand
/// </summary>
public class ConnectionUpdateCommandHandler : IRequestHandler<ConnectionUpdateCommand, IResult>
{
    private readonly IConnectionDal _connectionDal;
    private readonly INodeDal _nodeDal;
    private readonly IMessagesRepository _messagesRepository;

    public ConnectionUpdateCommandHandler(
        IConnectionDal connectionDal,
        INodeDal nodeDal,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _nodeDal = nodeDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to update a connection
    /// </summary>
    public async Task<IResult> Handle(ConnectionUpdateCommand request, CancellationToken cancellationToken)
    {
        // Get the connection
        var connection = await _connectionDal.GetAsync(c => c.Id == request.ConnectionDto.Id);
        if (connection == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Validate nodes if they are changed
        if (connection.SourceNodeId != request.ConnectionDto.SourceNodeId)
        {
            var sourceNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.SourceNodeId);
            if (sourceNode == null)
            {
                return new ErrorResult(
                    _messagesRepository.NotFound("Source node"),
                    HttpStatusCode.BadRequest);
            }
        }

        if (connection.TargetNodeId != request.ConnectionDto.TargetNodeId)
        {
            var targetNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.TargetNodeId);
            if (targetNode == null)
            {
                return new ErrorResult(
                    _messagesRepository.NotFound("Target node"),
                    HttpStatusCode.BadRequest);
            }
        }

        // Check for duplicates if nodes changed
        if ((connection.SourceNodeId != request.ConnectionDto.SourceNodeId || 
             connection.TargetNodeId != request.ConnectionDto.TargetNodeId) && 
            await _connectionDal.AnyAsync(c => 
                c.Id != request.ConnectionDto.Id &&
                c.SourceNodeId == request.ConnectionDto.SourceNodeId && 
                c.TargetNodeId == request.ConnectionDto.TargetNodeId))
        {
            return new ErrorResult(
                "A connection already exists between these nodes",
                HttpStatusCode.BadRequest);
        }

        // Update the connection
        connection.SourceNodeId = request.ConnectionDto.SourceNodeId;
        connection.TargetNodeId = request.ConnectionDto.TargetNodeId;
        connection.Label = request.ConnectionDto.Label;
        connection.ModifiedDate = DateTime.UtcNow;

        // Save to database
        await _connectionDal.UpdateAsync(connection);

        // Return success
        return new SuccessResult(_messagesRepository.Edited("Connection"));
    }
}