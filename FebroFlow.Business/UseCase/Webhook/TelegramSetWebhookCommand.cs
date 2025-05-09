using FebroFlow.Business.Services;
using FebroFlow.Core.ResultResponses;
using MediatR;

namespace FebroFlow.Business.UseCase.Webhook;

public class TelegramSetWebhookCommand : IRequest<IDataResult<object>>
{
    public string Url { get; }

    public TelegramSetWebhookCommand(string url)
    {
        Url = url;
    }
}

public class TelegramSetWebhookCommandHandler : IRequestHandler<TelegramSetWebhookCommand, IDataResult<object>>
{
    private readonly ITelegramService _telegramService;
    
    public TelegramSetWebhookCommandHandler(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }
    
    public async Task<IDataResult<object>> Handle(TelegramSetWebhookCommand request, CancellationToken cancellationToken)
    {
        var result = await _telegramService.SetWebhookAsync(request.Url);
        return result;
    }
}