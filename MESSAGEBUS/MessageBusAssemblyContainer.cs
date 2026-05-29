namespace MESSAGEBUS;

/// <summary>
///     Marker record used solely to locate the <c>MESSAGEBUS</c> assembly at startup. Pass
///     <c>typeof(MessageBusAssemblyContainer)</c> to any reflection-based registration (e.g.
///     a Scrutor scan for <c>IMessageHandler&lt;T&gt;</c> implementations declared in this
///     project).
/// </summary>
public record MessageBusAssemblyContainer;