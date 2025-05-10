using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;
using FebroFlow.Core.Responses;
using febroFlow.DataAccess.DataAccess;

namespace FebroFlow.Business.UseCase.Node;

public class ConnectionUpdateCommand : IRequest<IDataResult<object>>
{
    public ConnectionUpdateDto Form { get; }

    public ConnectionUpdateCommand(ConnectionUpdateDto form)
    {
        Form = form;
    }
}

public class ConnectionUpdateCommandHandler : IRequestHandler<ConnectionUpdateCommand, IDataResult<object>>
{
    private readonly IConnectionDal _connectionDal;
    private readonly IConnectionManager _connectionManager;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public ConnectionUpdateCommandHandler(
        IConnectionDal connectionDal,
        IConnectionManager connectionManager,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _connectionManager = connectionManager;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(ConnectionUpdateCommand request, CancellationToken cancellationToken)
    {
        // Check if connection exists
        var connection = await _connectionDal.GetAsync(x => x.Id == request.Form.Id);
        
        if (connection == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Connection"), HttpStatusCode.NotFound);
        }
        
        // If source or target node has changed, validate the new connection
        if (connection.SourceNodeId != request.Form.SourceNodeId || 
            connection.TargetNodeId != request.Form.TargetNodeId ||
            connection.Type != request.Form.Type)
        {
            var validationResult = await _connectionManager.ValidateConnectionAsync(
                request.Form.SourceNodeId,
                request.Form.TargetNodeId);
            
            if (validationResult is false)
            {
                return new ErrorDataResult<object>(validationResult, HttpStatusCode.BadRequest);
            }
        }
        
        // Update properties
        connection.SourceNodeId = request.Form.SourceNodeId;
        connection.TargetNodeId = request.Form.TargetNodeId;
        connection.Type = request.Form.Type;
        connection.SourceOutputIndex = request.Form.SourceOutputIndex;
        connection.TargetInputIndex = request.Form.TargetInputIndex;
        connection.ModifiedDate = DateTime.Now;
        
        // Save changes
        await _connectionDal.UpdateAsync(connection);
        
        return new SuccessDataResult<object>(connection, _messagesRepository.Edited("Connection"));
    }
}