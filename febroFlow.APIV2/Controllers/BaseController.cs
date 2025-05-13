using System.Net;
using FebroFlow.Core.ResultResponses;
using Microsoft.AspNetCore.Mvc;

namespace febroFlow.APIV2.Controllers;

public class BaseController: ControllerBase
{
    protected IActionResult Return(IDataResult<object> result)
    {
        return result.StatusCode switch
        {
            HttpStatusCode.BadRequest => BadRequest(result),
            HttpStatusCode.NotFound => NotFound(result),
            HttpStatusCode.Unauthorized => Unauthorized(result),
            HttpStatusCode.InternalServerError => BadRequest(result),
            HttpStatusCode.MethodNotAllowed => BadRequest(result),
            HttpStatusCode.Forbidden => StatusCode( 403),
            HttpStatusCode.NotAcceptable => StatusCode(406, result),
            _ => Ok(result)
        };
    }
}

public interface IResult
{
    public HttpStatusCode StatusCode { get; set; }
    public bool Result { get; }
    public string Message { get; }
    public string? ItemId { get; set; }
    public List<string> ErrorMessages { get; set; }
}