using System.Text.Json.Serialization;

namespace DTO.Responses;

/// <summary>
///     Default <see cref="IResult" /> implementation — record with init-only setters, so
///     instances are effectively immutable. Concrete <see cref="SuccessResult" /> /
///     <see cref="ErrorResult" /> set the boolean for callers.
/// </summary>
public record Result : IResult
{
    protected Result(bool success, string message)
        : this(success)
    {
        Message = message;
    }

    protected Result(bool success)
    {
        Success = success;
    }

    [JsonPropertyName("success")] public bool Success { get; init; }

    [JsonPropertyName("message")] public string? Message { get; init; }
}