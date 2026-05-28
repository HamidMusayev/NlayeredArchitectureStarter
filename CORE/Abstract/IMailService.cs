namespace CORE.Abstract;

public interface IMailService
{
    Task SendAsync(string email, string message, CancellationToken ct = default);
}