using System.Text;
using System.Text.Json;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Vector;
using MediatR;

namespace FebroFlow.Business.UseCase.Vector;

public class VectorSearchByTextCommand(string form, string collection) : IRequest<IDataResult<object>>
{
    public string Form { get; set; } = form;
    public string Collection { get; } = collection;
}

public class VectorSearchByTextCommandHandler(IPineconeService pineconeService) : IRequestHandler<VectorSearchByTextCommand, IDataResult<object>>
{
    public async Task<IDataResult<object>> Handle(VectorSearchByTextCommand request, CancellationToken cancellationToken)
    {
        var qdrantService = new QdrantServiceHelper(new HttpClient());
        var vectors = await pineconeService.GenerateEmbeddingAsync(request.Form);
        var results = await qdrantService.SearchAsync(request.Collection, vectors, 3, cancellationToken);
        return new SuccessDataResult<object>(results, "success");
    }
}