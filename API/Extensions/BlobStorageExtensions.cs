using CORE.Config;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STORAGE.Abstract;
using STORAGE.Concrete;

namespace API.Extensions;

/// <summary>
///     Registers the <see cref="STORAGE.Abstract.IBlobStorage" /> implementation chosen by
///     <c>BlobStorageSettings.Provider</c>: <c>Filesystem</c> (default), <c>Sftp</c>, or <c>S3</c>.
///     Derived projects swap storage back-ends by changing config — no code change required.
/// </summary>
public static class BlobStorageExtensions
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, ConfigSettings config)
    {
        switch (config.BlobStorageSettings.Provider)
        {
            case BlobStorageProvider.S3:
                services.TryAddSingleton<IBlobStorage, S3BlobStorage>();
                break;
            case BlobStorageProvider.Sftp:
                services.TryAddSingleton<IBlobStorage, SftpBlobStorage>();
                break;
            case BlobStorageProvider.Filesystem:
            default:
                services.TryAddSingleton<IBlobStorage, FilesystemBlobStorage>();
                break;
        }

        return services;
    }
}