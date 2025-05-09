using System.Net;
using System.Text.Json;
using febroFlow.Core.Models.Telegram;
using febroFlow.Core.ResultResponses;
using febroFlow.DataAccess.DataAccess;
using MediatR;
using Microsoft.Extensions.Logging;

namespace febroFlow.Business.UseCase.Webhook;

/// <summary>
/// Command to process a Telegram webhook update
/// </summary>
public class TelegramWebhookUpdateCommand : IRequest<IResult>
{
    public Update Update { get; }

    public TelegramWebhookUpdateCommand(Update update)
    {
        Update = update;
    }
}

/// <summary>
/// Handler for TelegramWebhookUpdateCommand
/// </summary>
public class TelegramWebhookUpdateCommandHandler : IRequestHandler<TelegramWebhookUpdateCommand, IResult>
{
    private readonly ITelegramService _telegramService;
    private readonly IFlowDal _flowDal;
    private readonly ILogger<TelegramWebhookUpdateCommandHandler> _logger;

    public TelegramWebhookUpdateCommandHandler(
        ITelegramService telegramService,
        IFlowDal flowDal,
        ILogger<TelegramWebhookUpdateCommandHandler> logger)
    {
        _telegramService = telegramService;
        _flowDal = flowDal;
        _logger = logger;
    }

    /// <summary>
    /// Handles the command to process a Telegram webhook update
    /// </summary>
    public async Task<IResult> Handle(TelegramWebhookUpdateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing Telegram update: {UpdateId}", request.Update.UpdateId);
            
            // Log the received update for debugging
            _logger.LogDebug("Received update: {Update}", JsonSerializer.Serialize(request.Update));
            
            // Process the update using the Telegram service
            await _telegramService.ProcessUpdate(request.Update);
            
            // Return success
            return new SuccessResult("Telegram webhook update processed successfully");
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "Error processing Telegram webhook update: {Message}", ex.Message);
            
            // Return error
            return new ErrorResult($"Error processing Telegram webhook update: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}