using System.Net;

namespace FebroFlow.Core.ResultResponses
{
    public class SuccessDataResult<T> : DataResult<T>
    {
        public SuccessDataResult(T data, bool result, string message, HttpStatusCode statusCode, List<string> errorMessages) : base(data, result, message, statusCode, errorMessages)
        {
        }
        public SuccessDataResult(T data, string message) : base(
            data: data,
            result: true, 
            message: message, 
            statusCode: HttpStatusCode.OK) { }

        // data, message and itemId
        public SuccessDataResult(T data, string message, string itemId) : base(
            data: data,
            result: true,
            message: message, 
            statusCode: HttpStatusCode.OK, 
            itemId: itemId) { }

        // data, message and itemId and returnUrl
        public SuccessDataResult(T data, string message, string returnUrl, string itemId) : base(
            data: data,
            result: true,
            message: message,
            returnUrl: returnUrl,
            itemId: itemId,
            statusCode: HttpStatusCode.OK) { }

        // default data and message
        public SuccessDataResult(string message) : base(
            data: default!,
            result: true,
            message: message,
            statusCode: HttpStatusCode.OK) { }
        
        // status code and message
        public SuccessDataResult(T data, HttpStatusCode statusCode, string message) : base(
            data: data,
            result: true,
            message: message,
            statusCode: statusCode) { }
    }
}