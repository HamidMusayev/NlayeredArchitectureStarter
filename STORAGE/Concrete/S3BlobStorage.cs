using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CORE.Config;
using Microsoft.Extensions.Options;
using STORAGE.Abstract;

namespace STORAGE.Concrete;

/// <summary>
///     AWS S3 blob backend. Also works against any S3-compatible store (MinIO, Cloudflare R2,
///     Backblaze B2 with S3 API, DigitalOcean Spaces) by pointing
///     <see cref="S3BlobSettings.ServiceUrl" /> at the alternate endpoint.
///     <para>
///         Container/key map onto S3 as <c>{container}/{key}</c> within the configured bucket —
///         containers become S3 key prefixes, not separate buckets, so bucket provisioning is
///         a one-time deploy concern.
///     </para>
/// </summary>
public sealed class S3BlobStorage : IBlobStorage, IDisposable
{
    private readonly Lazy<IAmazonS3> _client;
    private readonly S3BlobSettings _settings;

    public S3BlobStorage(IOptions<BlobStorageSettings> options)
    {
        _settings = options.Value.S3;
        _client = new Lazy<IAmazonS3>(BuildClient);
    }

    private string Bucket => _settings.BucketName;

    public async Task<string> SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        await _client.Value.PutObjectAsync(new PutObjectRequest
        {
            BucketName = Bucket,
            Key = ObjectKey(container, key),
            InputStream = content,
            ContentType = contentType,
            AutoCloseStream = false
        }, ct);

        return key;
    }

    public async Task<Stream> OpenAsync(string container, string key, CancellationToken ct = default)
    {
        try
        {
            var response = await _client.Value.GetObjectAsync(new GetObjectRequest
            {
                BucketName = Bucket,
                Key = ObjectKey(container, key)
            }, ct);

            // Buffer to a MemoryStream so callers can dispose the S3 response immediately.
            var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms, ct);
            ms.Position = 0;
            return ms;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException("Blob not found", $"{container}/{key}", ex);
        }
    }

    public async Task DeleteAsync(string container, string key, CancellationToken ct = default)
    {
        await _client.Value.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = Bucket,
            Key = ObjectKey(container, key)
        }, ct);
    }

    public async Task<bool> ExistsAsync(string container, string key, CancellationToken ct = default)
    {
        try
        {
            await _client.Value.GetObjectMetadataAsync(new GetObjectMetadataRequest
            {
                BucketName = Bucket,
                Key = ObjectKey(container, key)
            }, ct);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_client.IsValueCreated) _client.Value.Dispose();
    }

    private IAmazonS3 BuildClient()
    {
        var s = _settings;
        var credentials = new BasicAWSCredentials(s.AccessKey, s.SecretKey);

        var s3Config = new AmazonS3Config
        {
            ForcePathStyle = s.ForcePathStyle
        };

        if (!string.IsNullOrWhiteSpace(s.ServiceUrl))
        {
            s3Config.ServiceURL = s.ServiceUrl;
            s3Config.AuthenticationRegion = s.Region;
        }
        else
        {
            s3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(s.Region);
        }

        return new AmazonS3Client(credentials, s3Config);
    }

    private static string ObjectKey(string container, string key)
    {
        return $"{container.TrimEnd('/')}/{key.TrimStart('/')}";
    }
}