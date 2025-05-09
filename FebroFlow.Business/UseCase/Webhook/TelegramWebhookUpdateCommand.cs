using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
using FebroFlow.DataAccess.DbModels;
using MediatR;
using System.Net;
using System.Text.Json;

namespace FebroFlow.Business.UseCase.Webhook;

public class TelegramWebhookUpdateCommand : IRequest<IDataResult<object>>
{
    public string UpdateJson { get; }

    public TelegramWebhookUpdateCommand(string updateJson)
    {
        UpdateJson = updateJson;
    }
}

public class TelegramWebhookUpdateCommandHandler : IRequestHandler<TelegramWebhookUpdateCommand, IDataResult<object>>
{
    private readonly ITelegramService _telegramService;
    private readonly IFlowDal _flowDal;
    private readonly INodeDal _nodeDal;
    private readonly IFlowEngine _flowEngine;
    private readonly IChatMemoryDal _chatMemoryDal;
    
    public TelegramWebhookUpdateCommandHandler(
        ITelegramService telegramService,
        IFlowDal flowDal,
        INodeDal nodeDal,
        IFlowEngine flowEngine,
        IChatMemoryDal chatMemoryDal)
    {
        _telegramService = telegramService;
        _flowDal = flowDal;
        _nodeDal = nodeDal;
        _flowEngine = flowEngine;
        _chatMemoryDal = chatMemoryDal;
    }
    
    public async Task<IDataResult<object>> Handle(TelegramWebhookUpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Process the update using TelegramService
            var processResult = await _telegramService.ProcessUpdateAsync(request.UpdateJson);
            
            if (!processResult.Result)
            {
                return processResult;
            }
            
            // Extract chat ID from update
            var update = JsonDocument.Parse(request.UpdateJson).RootElement;
            
            // Check if it has a message
            if (!update.TryGetProperty("message", out var messageElement))
            {
                return new SuccessDataResult<object>("Unsupported update type", HttpStatusCode.OK);
            }
            
            // Get chat ID
            if (!messageElement.TryGetProperty("chat", out var chatElement) ||
                !chatElement.TryGetProperty("id", out var chatIdElement))
            {
                return new SuccessDataResult<object>("Chat ID not found", HttpStatusCode.OK);
            }
            
            string chatId = chatIdElement.ToString();
            
            // Find active flows with TelegramTrigger nodes
            var activeFlows = await _flowDal.GetAllAsync(f => f.IsActive);
            
            foreach (var flow in activeFlows)
            {
                // Find TelegramTrigger nodes for this flow
                var telegramTriggerNodes = await _nodeDal.GetAllAsync(n => 
                    n.FlowId == flow.Id && 
                    n.Type == Data.Enums.NodeType.TelegramTrigger);
                
                if (telegramTriggerNodes.Any())
                {
                    // Store message in chat memory
                    if (messageElement.TryGetProperty("text", out var textElement))
                    {
                        var chatMemory = new ChatMemory
                        {
                            ChatId = chatId,
                            Role = "user",
                            Content = textElement.GetString() ?? "",
                            FlowId = flow.Id,
                            Timestamp = DateTime.Now
                        };
                        
                        await _chatMemoryDal.AddAsync(chatMemory);
                    }
                    
                    // Execute flow for each trigger
                    foreach (var node in telegramTriggerNodes)
                    {
                        await _flowEngine.ExecuteFlowAsync(flow.Id, chatId, request.UpdateJson);
                    }
                }
            }
            
            return new SuccessDataResult<object>("Webhook processed successfully", HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}