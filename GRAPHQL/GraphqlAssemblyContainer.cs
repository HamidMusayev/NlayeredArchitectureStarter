namespace GRAPHQL;

/// <summary>
///     Marker record used solely to locate the <c>GRAPHQL</c> assembly at startup.
///     Pass <c>typeof(GraphqlAssemblyContainer)</c> when wiring HotChocolate so the
///     schema registration can discover types in this project.
/// </summary>
public record GraphqlAssemblyContainer;