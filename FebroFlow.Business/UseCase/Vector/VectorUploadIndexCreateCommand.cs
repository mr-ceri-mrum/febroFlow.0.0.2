using System.Text;
using System.Text.Json;
using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Vector;
using FebroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Vector;

public class VectorUploadCollectionCreateCommand(UploadVectorCollectionRequest form) : IRequest<IDataResult<object>>
{
    public UploadVectorCollectionRequest Form { get; set; } = form;
}

public class VectorUploadCollectionCreateCommandHandler(IQdrantService qdrantService, IPineconeService pineconeService, IVectorDal vectorDal) 
    
    : IRequestHandler<VectorUploadCollectionCreateCommand, IDataResult<object>>
{
    public async Task<IDataResult<object>> Handle(VectorUploadCollectionCreateCommand request, CancellationToken cancellationToken)
    {
        var qdrantServiceHelper = new QdrantServiceHelper(new HttpClient());
        string textContent;
        using (var reader = new StreamReader(request.Form.File.OpenReadStream()))
        {
            textContent = await reader.ReadToEndAsync(cancellationToken);
        }
        
        var chunks = SplitTextIntoChunks(textContent, chunkSize: 300, overlap: 30);
        
        var tasks = chunks.Select(async chunk =>
        {
            try
            {
                var embeddingAsync = await pineconeService.GenerateEmbeddingAsync(chunk);
                var point = new QdrantPoint
                {
                    Id = Guid.NewGuid().ToString(),
                    Vector = embeddingAsync.ToArray(),
                    Payload = new Dictionary<string, object>
                    {
                        { "text", chunk },
                        { "source", "С" }
                    }
                };
                await qdrantServiceHelper.UpsertPointAsync(request.Form.CollectionName, point, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing chunk: {ex.Message}");
                throw; 
            }
        });
        
        await Task.WhenAll(tasks);
        return new SuccessDataResult<object>("Success");
    }


    private List<string> SplitTextIntoChunks(string text, int chunkSize = 300, int overlap = 30)
    {
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();

        for (int i = 0; i < tokens.Length; i += chunkSize - overlap)
        {
            var chunkTokens = tokens.Skip(i).Take(chunkSize).ToArray();
            var chunk = string.Join(' ', chunkTokens);
            chunks.Add(chunk);
            if (i + chunkSize >= tokens.Length)
                break;
        }

        return chunks;
    }

}

