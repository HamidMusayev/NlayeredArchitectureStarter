namespace DTO.Responses;

/// <summary>2xx envelope with a typed payload. The default shape for read endpoints.</summary>
public record SuccessDataResult<T> : DataResult<T>
{
    public SuccessDataResult(T data, string message)
        : base(data, true, message)
    {
    }

    public SuccessDataResult(T data)
        : base(data, true)
    {
    }

    public SuccessDataResult(string message)
        : base(default, true, message)
    {
    }

    public SuccessDataResult()
        : base(default, true)
    {
    }
}