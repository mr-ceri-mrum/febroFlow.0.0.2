using FebroFlow.Business.UseCase.Flow;
using FebroFlow.Business.UseCase.Vector;
using FebroFlow.Data.Dtos.Flow;
using FebroFlow.Data.Dtos.Vector;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace febroFlow.APIV2.Controllers;

public class StorageController(IMediator mediator) : BaseController
{
    [HttpGet("GetCollections")]
    public async Task<IActionResult> GetCollections([FromQuery] string form)
    {
        var result = await mediator.Send(new VectorGetCollectionsCommand(form)); 
        return Return(result);
    }
    
    [HttpPost("CreateVectorIndex")]
    public async Task<IActionResult> CreateVectorIndex([FromQuery] CreateVectorIndexRequest form)
    {
        var result = await mediator.Send(new VectorIndexCreateCommand(form)); 
        return Return(result);
    }
    
    [HttpPost("UploadToVectorIndex")]
    public async Task<IActionResult> UploadToVector ([FromQuery] UploadVectorCollectionRequest form)
    {
        var result = await mediator.Send(new VectorUploadCollectionCreateCommand(form)); 
        return Return(result);
    }
    
    [HttpGet("SearchToVectorByText")]
    public async Task<IActionResult> SearchToVectorByText([FromQuery] string form, [FromQuery] string text)
    {
        var result = await mediator.Send(new VectorSearchByTextCommand(form, text)); 
        return Return(result);
    }
}