namespace DTO.Responses;

/// <summary>
///     Application result envelope — paired with concrete records (<see cref="Result" />,
///     <see cref="SuccessResult" />, <see cref="ErrorResult" />) to give controllers a
///     uniform success/message shape regardless of the underlying operation. 2xx happy paths
///     return this; 4xx/5xx exceptional paths use RFC 7807 ProblemDetails instead.
/// </summary>
public interface IResult
{
    bool Success { get; }

    string? Message { get; }
}