namespace MEDIATRS;

/// <summary>
///     Marker record used solely to locate the <c>MEDIATRS</c> assembly at startup.
///     Pass <c>typeof(MediatrAssemblyContainer)</c> to
///     <c>services.AddMediatR(cfg =&gt; cfg.RegisterServicesFromAssemblyContaining&lt;MediatrAssemblyContainer&gt;())</c>
///     so MediatR discovers all command/query handlers in this project.
/// </summary>
public record MediatrAssemblyContainer;