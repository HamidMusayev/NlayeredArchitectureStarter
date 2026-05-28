using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Http;
using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace CORE.Concrete;

public class SftpService(ConfigSettings configSettings) : ISftpService
{
    public async Task UploadFileAsync(string folderPath, string fileName, IFormFile formFile,
        CancellationToken ct = default)
    {
        using var sftpClient = new SftpClient(GetConnectionInfo());
        await sftpClient.ConnectAsync(ct);
        if (!sftpClient.IsConnected) return;

        await EnsureDirectoryAsync(sftpClient, folderPath, ct);

        var fullPath = CombinePath(folderPath, fileName);

        await using var sourceStream = formFile.OpenReadStream();
        await Task.Run(() => sftpClient.UploadFile(sourceStream, fullPath), ct);

        sftpClient.Disconnect();
    }

    public async Task DeleteFileAsync(string folderPath, string fileName, CancellationToken ct = default)
    {
        using var sftpClient = new SftpClient(GetConnectionInfo());
        await sftpClient.ConnectAsync(ct);
        if (!sftpClient.IsConnected) return;

        var fullPath = CombinePath(folderPath, fileName);
        if (await Task.Run(() => sftpClient.Exists(fullPath), ct))
            await Task.Run(() => sftpClient.DeleteFile(fullPath), ct);

        sftpClient.Disconnect();
    }

    public async Task<byte[]> ReadFileAsync(string folderPath, string fileName, CancellationToken ct = default)
    {
        using var sftpClient = new SftpClient(GetConnectionInfo());
        await sftpClient.ConnectAsync(ct);
        if (!sftpClient.IsConnected) return [];

        var fullPath = CombinePath(folderPath, fileName);

        await using var ms = new MemoryStream();
        await Task.Run(() => sftpClient.DownloadFile(fullPath, ms), ct);

        sftpClient.Disconnect();
        return ms.ToArray();
    }

    private ConnectionInfo GetConnectionInfo()
    {
        var authMethod = new PasswordAuthenticationMethod(
            configSettings.SftpSettings.UserName,
            configSettings.SftpSettings.Password);

        return new ConnectionInfo(
            configSettings.SftpSettings.Ip,
            configSettings.SftpSettings.UserName,
            authMethod);
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