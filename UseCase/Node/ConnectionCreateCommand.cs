using System.Net;
using febroFlow.Core.Dtos.Node;
using febroFlow.Core.Responses;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using febroFlow.DataAccess.DbModels;
using MediatR;

namespace febroFlow.Business.UseCase.Node;

/// <summary>
/// Command to create a new connection between nodes
/// </summary>
public class ConnectionCreateCommand : IRequest<IDataResult<Guid>>
{
    public ConnectionCreateDto ConnectionDto { get; }

    public ConnectionCreateCommand(ConnectionCreateDto connectionDto)
    {
        ConnectionDto = connectionDto;
    }
}

/// <summary>
/// Handler for ConnectionCreateCommand
/// </summary>
public class ConnectionCreateCommandHandler : IRequestHandler<ConnectionCreateCommand, IDataResult<Guid>>
{
    private readonly IConnectionDal _connectionDal;
    private readonly INodeDal _nodeDal;
    private readonly IMessagesRepository _messagesRepository;

    public ConnectionCreateCommandHandler(
        IConnectionDal connectionDal,
        INodeDal nodeDal,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _nodeDal = nodeDal;
        _messagesRepository = messagesRepository;
    }

    /// <summary>
    /// Handles the command to create a new connection
    /// </summary>
    public async Task<IDataResult<Guid>> Handle(ConnectionCreateCommand request, CancellationToken cancellationToken)
    {
        // Validate source node exists
        var sourceNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.SourceNodeId);
        if (sourceNode == null)
        {
            return new ErrorDataResult<Guid>(
                _messagesRepository.NotFound("Source node"),
                HttpStatusCode.BadRequest);
        }

        // Validate target node exists
        var targetNode = await _nodeDal.GetAsync(n => n.Id == request.ConnectionDto.TargetNodeId);
        if (targetNode == null)
        {
            return new ErrorDataResult<Guid>(
                _messagesRepository.NotFound("Target node"),
                HttpStatusCode.BadRequest);
        }

        // Check if connection already exists
        if (await _connectionDal.AnyAsync(c => 
            c.SourceNodeId == request.ConnectionDto.SourceNodeId && 
            c.TargetNodeId == request.ConnectionDto.TargetNodeId))
        {
            return new ErrorDataResult<Guid>(
                "A connection already exists between these nodes",
                HttpStatusCode.BadRequest);
        }

        // Create connection entity
        var connection = new Connection
        {
            Id = Guid.NewGuid(),
            SourceNodeId = request.ConnectionDto.SourceNodeId,
            TargetNodeId = request.ConnectionDto.TargetNodeId,
            Label = request.ConnectionDto.Label
        };

        // Save to database
        await _connectionDal.AddAsync(connection);

        // Return success result with the new connection ID
        return new SuccessDataResult<Guid>(connection.Id, _messagesRepository.Created("Connection"));
    }
}