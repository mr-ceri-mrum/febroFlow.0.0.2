using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using MediatR;

namespace FebroFlow.Business.UseCase.Vector;

public class VectorGetCollectionsCommand(string collectionName) : IRequest<IDataResult<object>>
{
    private string CollectionName { get; set; } = collectionName;
}

public class VectorGetByCollectionHandler : IRequestHandler<VectorGetCollectionsCommand, IDataResult<object>>
{
    public async Task<IDataResult<object>> Handle(VectorGetCollectionsCommand request, CancellationToken cancellationToken)
    {
        
        var qdrantServiceHelper = new QdrantServiceHelper(new HttpClient());
        var result = await qdrantServiceHelper.GetAllCollectionsAsync( cancellationToken);
        return new SuccessDataResult<object>(result, "Success");
    }
    
}

