namespace DTO.Responses;

/// <summary>
///     Business-rule failure envelope that still carries a (possibly partial) payload —
///     useful for "validation failed, here's the input echoed back" responses.
/// </summary>
public record ErrorDataResult<T> : DataResult<T>
{
    public ErrorDataResult(T data, string message)
        : base(data, false, message)
    {
    }

    public ErrorDataResult(T data)
        : base(data, false)
    {
    }

    public ErrorDataResult(string message)
        : base(default, false, message)
    {
    }

    public ErrorDataResult()
        : base(default, false)
    {
    }
}