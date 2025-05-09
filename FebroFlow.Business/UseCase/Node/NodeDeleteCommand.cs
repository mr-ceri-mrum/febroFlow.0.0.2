using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Node;

public class NodeDeleteCommand : IRequest<IDataResult<object>>
{
    public Guid Id { get; }

    public NodeDeleteCommand(Guid id)
    {
        Id = id;
    }
}

public class NodeDeleteCommandHandler : IRequestHandler<NodeDeleteCommand, IDataResult<object>>
{
    private readonly INodeDal _nodeDal;
    private readonly IConnectionDal _connectionDal;
    private readonly IMessagesRepository _messagesRepository;
    
    public NodeDeleteCommandHandler(
        INodeDal nodeDal,
        IConnectionDal connectionDal,
        IMessagesRepository messagesRepository)
    {
        _nodeDal = nodeDal;
        _connectionDal = connectionDal;
        _messagesRepository = messagesRepository;
    }
    
    public async Task<IDataResult<object>> Handle(NodeDeleteCommand request, CancellationToken cancellationToken)
    {
        // Check if node exists
        var node = await _nodeDal.GetAsync(x => x.Id == request.Id);
        
        if (node == null)
        {
            return new ErrorDataResult<object>(_messagesRepository.NotFound("Node"), HttpStatusCode.NotFound);
        }
        
        // Delete associated connections first
        await _connectionDal.DeleteAsync(x => 
            x.SourceNodeId == request.Id || x.TargetNodeId == request.Id);
        
        // Then delete the node
        await _nodeDal.DeleteAsync(node);
        
        return new SuccessDataResult<object>(true, _messagesRepository.Deleted());
    }
}