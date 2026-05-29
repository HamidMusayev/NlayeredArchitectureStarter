namespace DTO.Responses;

/// <summary>2xx envelope without a payload. Controllers return this from idempotent commands.</summary>
public record SuccessResult : Result
{
    public SuccessResult(string message)
        : base(true, message)
    {
    }

    public SuccessResult()
        : base(true)
    {
    }
}