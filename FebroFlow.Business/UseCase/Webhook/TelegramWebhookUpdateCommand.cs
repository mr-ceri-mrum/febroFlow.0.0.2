using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using FebroFlow.DataAccess.DataAccess;
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
    private readonly IFlowEngine _flowEngine;
    
    public TelegramWebhookUpdateCommandHandler(
        ITelegramService telegramService,
        IFlowDal flowDal,
        IFlowEngine flowEngine)
    {
        _telegramService = telegramService;
        _flowDal = flowDal;
        _flowEngine = flowEngine;
    }
    
    public async Task<IDataResult<object>> Handle(TelegramWebhookUpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Process update with Telegram service
            var processResult = await _telegramService.ProcessUpdateAsync(request.UpdateJson);
            
            if (!processResult.Result)
            {
                return processResult;
            }
            
            // Try to extract chat ID as context ID for flow execution
            string? contextId = null;
            try
            {
                var updateObject = JsonSerializer.Deserialize<JsonElement>(request.UpdateJson);
                
                if (updateObject.TryGetProperty("message", out var messageElement) &&
                    messageElement.TryGetProperty("chat", out var chatElement) &&
                    chatElement.TryGetProperty("id", out var chatIdElement))
                {
                    contextId = chatIdElement.ToString();
                }
            }
            catch
            {
                // If we can't parse the JSON or extract chat ID, just continue
            }
            
            if (string.IsNullOrEmpty(contextId))
            {
                return new SuccessDataResult<object>(processResult.Data, "Webhook update processed, but no context ID found for flow execution");
            }
            
            // Find flows with Telegram triggers to execute
            var telegramFlows = await _flowDal.GetAllAsync(x => x.IsActive);
            
            foreach (var flow in telegramFlows)
            {
                // Check if flow has a Telegram trigger node
                // This is a simplified check - in a real implementation,
                // you would check the nodes in the flow for a Telegram trigger type
                
                // For demonstration, execute all active flows with the webhook data
                await _flowEngine.ExecuteFlowAsync(flow.Id, contextId, request.UpdateJson);
            }
            
            return new SuccessDataResult<object>(processResult.Data, "Webhook update processed and flows executed");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}