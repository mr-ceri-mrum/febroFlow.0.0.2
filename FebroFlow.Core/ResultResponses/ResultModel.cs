using System.Net;
using FebroFlow.Core.Responses;

namespace FebroFlow.Core.ResultResponses;

public class ResultModel : IResult
{
    public bool Result { get; }
    public string Message { get; }
    public HttpStatusCode StatusCode { get; set; }
    public string? ItemId { get; set; }
    public List<string> ErrorMessages { get; set; }
    public string? ReturnUrl { get; set; }


    public ResultModel(bool Result, HttpStatusCode statusCode)
    {
        this.Result = Result;
        this.StatusCode = statusCode;
    }

    public ResultModel(bool Result, string Message, HttpStatusCode statusCode) : this(Result, statusCode)
    {
        this.Result = Result;
        this.Message = Message;
        this.StatusCode = statusCode;
    }

    public ResultModel(bool Result, string Message, HttpStatusCode statusCode, string itemId)
    {
        this.Result = Result;
        this.Message = Message;
        this.StatusCode = statusCode;
        this.ItemId = itemId;
    }

    public ResultModel(bool Result, string Message, HttpStatusCode statusCode, string itemId, string ReturnUrl)
    {
        this.Result = Result;
        this.Message = Message;
        this.StatusCode = statusCode;
        this.ItemId = itemId;
        this.ReturnUrl = ReturnUrl;
    }
    public ResultModel(bool Result, string Message, HttpStatusCode statusCode, string itemId, string ReturnUrl, List<string> errorMessages)
    {
        this.Result = Result;
        this.Message = Message;
        this.StatusCode = statusCode;
        this.ItemId = itemId;
        this.ReturnUrl = ReturnUrl;
        this.ErrorMessages = errorMessages;
    }
}