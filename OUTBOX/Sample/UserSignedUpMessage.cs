namespace OUTBOX.Sample;

/// <summary>
///     Sample integration message used by the Outbox demo endpoint. Carries the identifiers a
///     downstream subscriber would need to react to a new user (send welcome email, provision
///     resources, fan-out to analytics, …). Records here are the unit of work between the
///     outbox writer and any <c>IMessageHandler&lt;UserSignedUpMessage&gt;</c>.
/// </summary>
public sealed record UserSignedUpMessage(Guid UserId, string Email, DateTimeOffset OccurredOnUtc);