using AutoMapper;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Node;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Node;

public class ConnectionCreateCommand : IRequest<IDataResult<object>>
{
    public ConnectionCreateDto Form { get; }

    public ConnectionCreateCommand(ConnectionCreateDto form)
    {
        Form = form;
    }
}

public class ConnectionCreateCommandHandler : IRequestHandler<ConnectionCreateCommand, IDataResult<object>>
{
    private readonly IConnectionDal _connectionDal;
    private readonly IFlowDal _flowDal;
    private readonly IConnectionManager _connectionManager;
    private readonly IMapper _mapper;
    private readonly IMessagesRepository _messagesRepository;
    
    public ConnectionCreateCommandHandler(
        IConnectionDal connectionDal,
        IFlowDal flowDal,
        IConnectionManager connectionManager,
        IMapper mapper,
        IMessagesRepository messagesRepository)
    {
        _connectionDal = connectionDal;
        _flowDal = flowDal;
        _connectionManager = connectionManager;
        _mapper = mapper;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(ConnectionCreateCommand request, CancellationToken cancellationToken)
    {
        // Check if flow exists
        var flow = await _flowDal.GetAsync(x => x.Id == request.Form.FlowId);
        
        if (flow == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Flow"), HttpStatusCode.NotFound);
        }
        
        // Validate connection
        var validationResult = await _connectionManager.ValidateConnectionAsync(
            request.Form.SourceNodeId,
            request.Form.TargetNodeId,
            request.Form.Type);
        
        if (!validationResult.Result || validationResult.Data == false)
        {
            return new ErrorDataResult<object>(validationResult.Message, HttpStatusCode.BadRequest);
        }
        
        // Map DTO to entity
        var connection = _mapper.Map<Connection>(request.Form);
        
        // Set creation data
        connection.DataCreate = DateTime.Now;
        
        // Save to database
        await _connectionDal.AddAsync(connection);
        
        return new SuccessDataResult<object>(connection, _messagesRepository.Created("Connection"));
    }
}