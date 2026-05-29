namespace DTO.Responses;

/// <summary>
///     <see cref="IResult" /> that also carries a typed payload. Covariant in <typeparamref name="T" />.
/// </summary>
public interface IDataResult<out T> : IResult
{
    T? Data { get; }
}