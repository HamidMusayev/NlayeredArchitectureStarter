namespace STORAGE.Abstract;

/// <summary>
///     One vendor-neutral surface for byte-storage. Implementations ship under
///     <c>STORAGE.Concrete</c>: filesystem, SFTP, S3 (MinIO-compatible).
///     Selected at startup via <see cref="CORE.Config.BlobStorageSettings.Provider" />.
///     <para>
///         Use <paramref name="container" /> for the logical group (e.g. <c>"UserProfile"</c>,
///         <c>"OrganizationLogo"</c>) and <paramref name="key" /> for the per-object identity
///         (typically <c>{hash}{extension}</c>). The implementation maps these to whatever the
///         backing store uses — a directory, an S3 prefix, an SFTP path, etc.
///     </para>
/// </summary>
public interface IBlobStorage
{
    /// <summary>
    ///     Writes <paramref name="content" /> to <paramref name="container" />/<paramref name="key" />,
    ///     overwriting if it already exists. Returns the canonical key the impl used (usually
    ///     the input <paramref name="key" />).
    /// </summary>
    Task<string> SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    ///     Opens a readable stream over the object. Caller must dispose. Throws
    ///     <see cref="FileNotFoundException" /> if the object is missing.
    /// </summary>
    Task<Stream> OpenAsync(string container, string key, CancellationToken ct = default);

    /// <summary>
    ///     Deletes the object. No-op if it doesn't exist.
    /// </summary>
    Task DeleteAsync(string container, string key, CancellationToken ct = default);

    /// <summary>
    ///     True if the object exists; false otherwise. Cheap check — does not transfer bytes.
    /// </summary>
    Task<bool> ExistsAsync(string container, string key, CancellationToken ct = default);
}