using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using MediatR;
using System.Net;

namespace FebroFlow.Business.UseCase.Webhook;

public class TelegramDeleteWebhookCommand : IRequest<IDataResult<object>>
{
}

public class TelegramDeleteWebhookCommandHandler : IRequestHandler<TelegramDeleteWebhookCommand, IDataResult<object>>
{
    private readonly ITelegramService _telegramService;
    
    public TelegramDeleteWebhookCommandHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }
    
    public async Task<IDataResult<object>> Handle(TelegramDeleteWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _telegramService.DeleteWebhookAsync();
            return result;
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<object>(ex.Message, HttpStatusCode.InternalServerError);
        }
    }
}