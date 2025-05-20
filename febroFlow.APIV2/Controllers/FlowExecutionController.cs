using System.Net;
using System.Text.Json;
using FebroFlow.Business.Services;
using FebroFlow.Business.UseCase.Flow;
using FebroFlow.Core.ResultResponses;
using FebroFlow.Data.Dtos.Flow;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace febroFlow.APIV2.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FlowExecutionController(IMediator mediator) : BaseController
{
    [HttpPost("CreateFlowExecution")]
    public async Task<IActionResult> CreateFlowExecution([FromForm] CreateFlowRequest form)
    {
        var result = await mediator.Send(new FlowCreateCommand(form)); 
        return Return(result);
    }
    
    [HttpPost("TestFlow")]
    public async Task<IActionResult> TestFlow([FromQuery] TestFlowRequest form)
    {
        var result = await mediator.Send(new FlowTestCommand(form)); 
        return Return(result);
    }
}

