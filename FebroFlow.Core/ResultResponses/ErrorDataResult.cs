using System.Net;

namespace FebroFlow.Core.ResultResponses
{
    public class ErrorDataResult<T> : DataResult<T>
    {
        // message, status code and error messages
        public ErrorDataResult(string message, HttpStatusCode statusCode, List<string> errorMessages)
            : base(default!, false, message, statusCode, errorMessages)
        {
        }
        
        // data, message and status code
        public ErrorDataResult(T data, string message, HttpStatusCode statusCode) 
            : base(data, false, message,
                statusCode)
        {
        }

        // message and status code
        public ErrorDataResult(string message, HttpStatusCode statusCode) 
            : base(default!, false, message, statusCode)
        {
        }
    }
}