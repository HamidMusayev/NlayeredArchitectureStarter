namespace DTO.Responses;

/// <summary>
///     Business-rule failure envelope (validation, not-found, etc.). Distinct from the RFC 7807
///     ProblemDetails path which handles unhandled exceptions — this is a *known* failure the
///     service is reporting deliberately.
/// </summary>
public record ErrorResult : Result
{
    public ErrorResult(string message)
        : base(false, message)
    {
    }

    public ErrorResult()
        : base(false)
    {
    }
}