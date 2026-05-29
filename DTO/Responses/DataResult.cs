namespace DTO.Responses;

/// <summary>
///     Default <see cref="IDataResult{T}" /> implementation. <see cref="SuccessDataResult{T}" />
///     and <see cref="ErrorDataResult{T}" /> derive from this with the success flag baked in.
/// </summary>
public record DataResult<T> : Result, IDataResult<T>
{
    protected DataResult(T? data, bool success, string message)
        : base(success, message)
    {
        Data = data;
    }

    protected DataResult(T? data, bool success)
        : base(success)
    {
        Data = data;
    }

    public T? Data { get; init; }
}