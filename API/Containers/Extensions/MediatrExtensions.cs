using MEDIATRS;

namespace API.Containers.Extensions;

/// <summary>
///     Registers MediatR by scanning the MEDIATRS assembly (anchored on
///     <see cref="MEDIATRS.MediatrAssemblyContainer" />) for all
///     <c>IRequestHandler&lt;,&gt;</c> implementations.
/// </summary>
public static class MediatrExtensions
{
    public static IServiceCollection AddMediatrHandlers(this IServiceCollection services)
    {
        // AddMediatR scans for IRequestHandler<,> implementations on its own; don't double-scan with Scrutor.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatrAssemblyContainer>());
        return services;
    }
}