using CORE.Config;
using Renci.SshNet;
using STORAGE.Abstract;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace STORAGE.Concrete;

/// <summary>
///     <see cref="IBlobStorage" /> implementation that talks directly to an SFTP server via
///     SSH.NET. Useful when a deployment mandates SFTP for compliance reasons. Connection
///     details come from <see cref="SftpSettings" />; one short-lived <see cref="SftpClient" />
///     is created per operation (no pooling — the call rate for file uploads is low).
/// </summary>
public sealed class SftpBlobStorage(ConfigSettings config) : IBlobStorage
{
    public async Task<string> SaveAsync(
        string container,
        string key,
        Stream content,
        string contentType,
        CancellationToken ct = default)
    {
        using var client = new SftpClient(GetConnectionInfo());
        await client.ConnectAsync(ct);
        if (!client.IsConnected) throw new IOException("Failed to connect to SFTP server.");

        await EnsureDirectoryAsync(client, container, ct);

        var fullPath = CombinePath(container, key);
        await Task.Run(() => client.UploadFile(content, fullPath), ct);

        client.Disconnect();
        return key;
    }

    public async Task<Stream> OpenAsync(string container, string key, CancellationToken ct = default)
    {
        using var client = new SftpClient(GetConnectionInfo());
        await client.ConnectAsync(ct);
        if (!client.IsConnected) throw new IOException("Failed to connect to SFTP server.");

        var fullPath = CombinePath(container, key);
        if (!await Task.Run(() => client.Exists(fullPath), ct))
            throw new FileNotFoundException("Blob not found", $"{container}/{key}");

        var buffer = new MemoryStream();
        await Task.Run(() => client.DownloadFile(fullPath, buffer), ct);
        client.Disconnect();

        buffer.Position = 0;
        return buffer;
    }

    public async Task DeleteAsync(string container, string key, CancellationToken ct = default)
    {
        using var client = new SftpClient(GetConnectionInfo());
        await client.ConnectAsync(ct);
        if (!client.IsConnected) return;

        var fullPath = CombinePath(container, key);
        if (await Task.Run(() => client.Exists(fullPath), ct))
            await Task.Run(() => client.DeleteFile(fullPath), ct);

        client.Disconnect();
    }

    public async Task<bool> ExistsAsync(string container, string key, CancellationToken ct = default)
    {
        using var client = new SftpClient(GetConnectionInfo());
        await client.ConnectAsync(ct);
        if (!client.IsConnected) return false;

        var fullPath = CombinePath(container, key);
        var exists = await Task.Run(() => client.Exists(fullPath), ct);

        client.Disconnect();
        return exists;
    }

    private ConnectionInfo GetConnectionInfo()
    {
        var auth = new PasswordAuthenticationMethod(
            config.SftpSettings.UserName,
            config.SftpSettings.Password);

        return new ConnectionInfo(
            config.SftpSettings.Ip,
            config.SftpSettings.UserName,
            auth);
    }

    private static async Task EnsureDirectoryAsync(SftpClient client, string folderPath, CancellationToken ct)
    {
        if (!await Task.Run(() => client.Exists(folderPath), ct))
            await Task.Run(() => client.CreateDirectory(folderPath), ct);
    }

    private static string CombinePath(string folder, string file)
    {
        return folder.TrimEnd('/') + "/" + file;
    }
}