using MESSAGEBUS.Abstract;
using Microsoft.Extensions.Logging;

namespace OUTBOX.Sample;

/// <summary>
///     Demo <see cref="IMessageHandler{T}" /> for <see cref="UserSignedUpMessage" />. Registered
///     in DI by <c>OutboxExtensions.AddOutbox</c> and resolved by the message bus on every
///     published message. Real handlers would send a welcome email, fan-out to analytics,
///     etc. — this one just logs.
/// </summary>
public sealed class UserSignedUpHandler(ILogger<UserSignedUpHandler> logger)
    : IMessageHandler<UserSignedUpMessage>
{
    public Task HandleAsync(UserSignedUpMessage message, CancellationToken ct)
    {
        logger.LogInformation(
            "UserSignedUp handled — UserId={UserId} Email={Email} OccurredOnUtc={OccurredOnUtc}",
            message.UserId, message.Email, message.OccurredOnUtc);
        return Task.CompletedTask;
    }
}