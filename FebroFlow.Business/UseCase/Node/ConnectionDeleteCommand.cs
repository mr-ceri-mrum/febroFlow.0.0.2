using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Node;

public class ConnectionDeleteCommand : IRequest<IDataResult<object>>
{
    public Guid Id { get; }

    public ConnectionDeleteCommand(Guid id)
    {
        Id = id;
    }
}

public class ConnectionDeleteCommandHandler : IRequestHandler<ConnectionDeleteCommand, IDataResult<object>>
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
    
    public async Task<IDataResult<object>> Handle(ConnectionDeleteCommand request, CancellationToken cancellationToken)
    {
        // Check if connection exists
        var connection = await _connectionDal.GetAsync(x => x.Id == request.Id);
        
        if (connection == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Connection"), HttpStatusCode.NotFound);
        }
        
        // Delete the connection
        await _connectionDal.DeleteAsync(connection);
        
        return new SuccessDataResult<object>(true, _messagesRepository.Deleted());
    }
}