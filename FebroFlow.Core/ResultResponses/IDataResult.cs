using FebroFlow.Core.Responses;

namespace FebroFlow.Core.ResultResponses;

public interface IDataResult<T> : IResult
{
    T Data { get; }
}