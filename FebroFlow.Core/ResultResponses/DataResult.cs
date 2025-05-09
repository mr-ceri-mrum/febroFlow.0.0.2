using System.Net;

namespace FebroFlow.Core.ResultResponses
{
    public class DataResult<T> : ResultModel, IDataResult<T>
    {
        public T Data { get; }

        public DataResult(T data, bool result, string message, HttpStatusCode statusCode, List<string> errorMessages)
        : base(result, statusCode)
        {
            Data = data;
        }
        
        public DataResult(T data, bool result, string message, HttpStatusCode statusCode) 
            : base(result, message, statusCode)
        {
            Data = data;
        }
        
        public DataResult(T data, bool result, string message, string returnUrl, string itemId, HttpStatusCode statusCode) 
            : base(result, message, statusCode, itemId, returnUrl)
        {
            Data = data;
        }

        public DataResult(T data, bool result, string message, HttpStatusCode statusCode, string itemId) 
            : base(result, message, statusCode, itemId)
        {
            Data = data;
        }
    }
}