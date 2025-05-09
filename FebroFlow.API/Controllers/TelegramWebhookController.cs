using FebroFlow.Business.UseCase.Webhook;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FebroFlow.API.Controllers;

[Route("api/[controller]/")]
[ApiController]
public class TelegramWebhookController : BaseController
{
    private readonly IMediator _mediator;
    
    public TelegramWebhookController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("Update")]
    public async Task<IActionResult> Update([FromBody] string updateJson)
    {
        var result = await _mediator.Send(new TelegramWebhookUpdateCommand(updateJson));
        return Return(result);
    }
    
    [HttpPost("SetWebhook")]
    public async Task<IActionResult> SetWebhook([FromQuery] string url)
    {
        var result = await _mediator.Send(new TelegramSetWebhookCommand(url));
        return Return(result);
    }
    
    [HttpPost("DeleteWebhook")]
    public async Task<IActionResult> DeleteWebhook()
    {
        var result = await _mediator.Send(new TelegramDeleteWebhookCommand());
        return Return(result);
    }
}