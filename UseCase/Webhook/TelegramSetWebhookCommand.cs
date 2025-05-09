using System.Net;
using febroFlow.Core.Dtos.Webhook;
using febroFlow.Core.ResultResponses;
using MediatR;

namespace febroFlow.Business.UseCase.Webhook;

/// <summary>
/// Command to set a webhook for the Telegram bot
/// </summary>
public class TelegramSetWebhookCommand : IRequest<IResult>
{
    public TelegramWebhookDto WebhookDto { get; }

    public TelegramSetWebhookCommand(TelegramWebhookDto webhookDto)
    {
        WebhookDto = webhookDto;
    }
}

/// <summary>
/// Handler for TelegramSetWebhookCommand
/// </summary>
public class TelegramSetWebhookCommandHandler : IRequestHandler<TelegramSetWebhookCommand, IResult>
{
    private readonly ITelegramService _telegramService;
    private readonly IWebhookDal _webhookDal;

    public TelegramSetWebhookCommandHandler(
        ITelegramService telegramService,
        IWebhookDal webhookDal)
    {
        _telegramService = telegramService;
        _webhookDal = webhookDal;
    }

    /// <summary>
    /// Handles the command to set a Telegram webhook
    /// </summary>
    public async Task<IResult> Handle(TelegramSetWebhookCommand request, CancellationToken cancellationToken)
    {
        // Set the webhook with Telegram
        var success = await _telegramService.SetWebhook(
            request.WebhookDto.Url,
            request.WebhookDto.AllowedUpdates?.ToList());

        if (!success)
        {
            return new ErrorResult("Failed to set Telegram webhook", HttpStatusCode.InternalServerError);
        }

        // Create or update webhook record in database
        var existingWebhook = await _webhookDal.GetAsync(w => w.ServiceType == "Telegram");
        
        if (existingWebhook != null)
        {
            // Update
            existingWebhook.Url = request.WebhookDto.Url;
            existingWebhook.Config = System.Text.Json.JsonSerializer.Serialize(request.WebhookDto);
            existingWebhook.ModifiedDate = DateTime.UtcNow;
            
            await _webhookDal.UpdateAsync(existingWebhook);
        }
        else
        {
            // Create
            var webhook = new DataAccess.DbModels.Webhook
            {
                Id = Guid.NewGuid(),
                ServiceType = "Telegram",
                Url = request.WebhookDto.Url,
                Config = System.Text.Json.JsonSerializer.Serialize(request.WebhookDto),
                IsActive = true
            };
            
            await _webhookDal.AddAsync(webhook);
        }

        return new SuccessResult("Telegram webhook set successfully");
    }
}