using CORE.Config;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using STORAGE.Abstract;

namespace STORAGE.Concrete;

/// <summary>
///     Writes blobs to the local filesystem under <see cref="FilesystemBlobSettings.RootPath" />.
///     Default for new projects — no external infrastructure required. Useful in dev/CI and as
///     the fallback in small deployments. Swap to <c>S3BlobStorage</c> when going multi-instance.
/// </summary>
public sealed class FilesystemBlobStorage(
    IOptions<BlobStorageSettings> options,
    IHostEnvironment env) : IBlobStorage
{
    private readonly FilesystemBlobSettings _fs = options.Value.Filesystem;

    private string Root
    {
        get
        {
            var configured = _fs.RootPath;
            return Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(env.ContentRootPath, configured);
        }
    }

    public async Task<string> SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        var dir = Path.Combine(Root, Sanitize(container));
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, Sanitize(key));
        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fs, ct);

        return key;
    }

    public Task<Stream> OpenAsync(string container, string key, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(Root, Sanitize(container), Sanitize(key));
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Blob not found", fullPath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string container, string key, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(Root, Sanitize(container), Sanitize(key));
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string container, string key, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(Root, Sanitize(container), Sanitize(key));
        return Task.FromResult(File.Exists(fullPath));
    }

    /// <summary>
    ///     Strips directory-traversal sequences from caller-supplied container / key segments.
    ///     The starter only ever passes opaque hashed names + an enum-derived container, but
    ///     defense-in-depth keeps a future caller from sliding <c>"../../etc/passwd"</c>
    ///     through the API.
    /// </summary>
    private static string Sanitize(string segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
            throw new ArgumentException("Container/key segment must not be empty", nameof(segment));

        var trimmed = segment.Replace("..", string.Empty).Trim('/', '\\');
        return trimmed.Length == 0
            ? throw new ArgumentException("Container/key segment must not be empty after sanitization", nameof(segment))
            : trimmed;
    }
}