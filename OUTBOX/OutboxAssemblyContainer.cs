namespace OUTBOX;

/// <summary>
///     Marker record used solely to locate the <c>OUTBOX</c> assembly at startup. Pass
///     <c>typeof(OutboxAssemblyContainer)</c> to any reflection-based registration that needs
///     to scan this project (for example, future Scrutor scans for additional
///     <c>IMessageHandler&lt;T&gt;</c> implementations).
/// </summary>
public record OutboxAssemblyContainer;