using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using MediatR;

namespace FebroFlow.Business.UseCase.Node;

public class NodeGetTypesCommand : IRequest<IDataResult<object>>
{
}

public class NodeGetTypesCommandHandler : IRequestHandler<NodeGetTypesCommand, IDataResult<object>>
{
    private readonly INodeFactory _nodeFactory;
    
    public NodeGetTypesCommandHandler(INodeFactory nodeFactory)
    {
        _nodeFactory = nodeFactory;
    }
    
    public async Task<IDataResult<object>> Handle(NodeGetTypesCommand request, CancellationToken cancellationToken)
    {
        //var result = await _nodeFactory.GetAvailableNodeTypesAsync();
        return null;
    }
}