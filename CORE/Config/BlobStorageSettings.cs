namespace CORE.Config;

/// <summary>
///     Selects the active <c>IBlobStorage</c> implementation and supplies per-provider settings.
///     Default is <see cref="BlobStorageProvider.Filesystem" /> — zero infrastructure dependency.
/// </summary>
public record BlobStorageSettings
{
    public BlobStorageProvider Provider { get; set; } = BlobStorageProvider.Filesystem;

    /// <summary>
    ///     Filesystem-provider config. Root directory under which containers/objects are written.
    ///     Relative paths are resolved against the application's content root (i.e. inside
    ///     <c>wwwroot</c> when set to <c>"wwwroot/blobs"</c>).
    /// </summary>
    public FilesystemBlobSettings Filesystem { get; set; } = new();

    /// <summary>
    ///     S3-provider config (also covers MinIO when <see cref="S3BlobSettings.ServiceUrl" />
    ///     is set to a self-hosted endpoint).
    /// </summary>
    public S3BlobSettings S3 { get; set; } = new();
    public SftpSettings Sftp { get; set; } = new();
}

public enum BlobStorageProvider
{
    Filesystem = 0,
    Sftp = 1,
    S3 = 2
}

public record FilesystemBlobSettings
{
    public string RootPath { get; set; } = "wwwroot/blobs";
}

public record S3BlobSettings
{
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    ///     Custom S3 endpoint. Leave empty for AWS S3; set to e.g. <c>http://localhost:9000</c>
    ///     for MinIO or other S3-compatible stores.
    /// </summary>
    public string? ServiceUrl { get; set; }

    /// <summary>
    ///     MinIO requires path-style requests; AWS supports either. Default <c>true</c>
    ///     works for both.
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;
}

/// <summary>SFTP credentials used by <c>SftpService</c> + <c>SftpBlobStorage</c>.</summary>
public record SftpSettings
{
    public  string UserName { get; set; }= string.Empty;
    public  string Ip { get; set; }= string.Empty;
    public  string Password { get; set; }= string.Empty;
}