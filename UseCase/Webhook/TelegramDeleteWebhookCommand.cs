using System.Net;
using febroFlow.Core.ResultResponses;
using MediatR;

namespace febroFlow.Business.UseCase.Webhook;

/// <summary>
/// Command to delete the Telegram bot webhook
/// </summary>
public class TelegramDeleteWebhookCommand : IRequest<IResult>
{
}

/// <summary>
/// Handler for TelegramDeleteWebhookCommand
/// </summary>
public class TelegramDeleteWebhookCommandHandler : IRequestHandler<TelegramDeleteWebhookCommand, IResult>
{
    private readonly ITelegramService _telegramService;

    public TelegramDeleteWebhookCommandHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    /// <summary>
    /// Handles the command to delete a Telegram webhook
    /// </summary>
    public async Task<IResult> Handle(TelegramDeleteWebhookCommand request, CancellationToken cancellationToken)
    {
        var success = await _telegramService.DeleteWebhook();
        
        if (success)
        {
            return new SuccessResult("Telegram webhook deleted successfully");
        }
        
        return new ErrorResult("Failed to delete Telegram webhook", HttpStatusCode.InternalServerError);
    }
}