using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using febroFlow.DataAccess.DataAccess;
using MediatR;

namespace FebroFlow.Business.UseCase.Flow;

public class FlowTestCommand (TestFlowRequest form): IRequest<IDataResult<object>>
{
    public TestFlowRequest Form { get; set; } = form;
}

public class FlowTestCommandHandler(IFlowDal flowDal, IMessagesRepository messagesRepository
, IPineconeService pineconeService, IOpenAiService openAiService 
) 
    
    : IRequestHandler<FlowTestCommand, IDataResult<object>>
{
    public async Task<IDataResult<object>> Handle(FlowTestCommand request, CancellationToken cancellationToken)
    {
        var flow = await flowDal.GetAsync(x => x.Id == request.Form.Id || !x.IsActive);
        if(flow == null || string.IsNullOrWhiteSpace(request.Form.Text)) return new ErrorDataResult<object>("Flow not found", HttpStatusCode.NotFound);
        
        QdrantServiceHelper qdrantServiceHelper = new QdrantServiceHelper(new HttpClient());
        var embedding = await pineconeService.GenerateEmbeddingAsync(request.Form.Text);
        
        var relatedChunks = await qdrantServiceHelper.SearchAsync(request.Form.collectionName, embedding, cancellationToken: cancellationToken);
        
        var context = string.Join("\n\n", relatedChunks
            .Where(chunk => chunk.Payload != null)
            .SelectMany(chunk => chunk.Payload.Values)
            .Select(val => val?.ToString())
            .Where(val => !string.IsNullOrWhiteSpace(val)));
        
        return new SuccessDataResult<object>
        (await openAiService.SendPromptAsync(flow.SysteamPromt, context, request.Form.Text), 
            "Vector stored, queried and answered.");
    }

    

} 