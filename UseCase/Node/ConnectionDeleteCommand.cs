using System.Net;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to delete a connection by ID
/// </summary>
public class ConnectionDeleteCommand : IRequest<IResult>
{
    public Guid ConnectionId { get; }

    public ConnectionDeleteCommand(Guid connectionId)
    {
        ConnectionId = connectionId;
    }
}

/// <summary>
/// Handler for ConnectionDeleteCommand
/// </summary>
public class ConnectionDeleteCommandHandler : IRequestHandler<ConnectionDeleteCommand, IResult>
{
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;

    public ConnectionDeleteCommandHandler(
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to delete a connection
    /// </summary>
    public async Task<IResult> Handle(ConnectionDeleteCommand request, CancellationToken cancellationToken)
    {
        // Get the connection
        var connection = await _connectionDal.GetAsync(c => c.Id == request.ConnectionId);
        if (connection == null)
        {
            return new ErrorResult(
                _messagesRepository.NotFound(),
                HttpStatusCode.NotFound);
        }

        // Delete the connection
        await _connectionDal.DeleteAsync(connection);

        // Return success
        return new SuccessResult(_messagesRepository.Deleted());
    }
}