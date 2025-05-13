using System.Net;
using FebroFlow.Business.Services;
using FebroFlow.Core.Responses;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using febroFlow.DataAccess.DataAccess;
using MediatR;
using Pinecone;

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
        
        var embedding = await pineconeService.GenerateEmbeddingAsync(request.Form.Text); 
        var relatedChunks = await pineconeService.QueryAsync(embedding.ToArray(), topK: 20); 
        
        var context = string.Join("\n", relatedChunks); 
        var prompt = $"""
                      Используя следующий контекст, ответь на вопрос:

                      Контекст:
                      {context}

                      Вопрос:
                      {request.Form.Text}

                      Ответ:
                      """;
        
        var completion = await openAiService.GetChatCompletionAsync(prompt);
        
        
        return new SuccessDataResult<object>(completion, "Vector stored, queried and answered.");
    }
} 