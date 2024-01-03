using CORE.Abstract;
using CORE.Config;
using Microsoft.AspNetCore.Http;
using Renci.SshNet;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace CORE.Concrete;

public class SftpService(ConfigSettings configSettings) : ISftpService
{
    public void UploadFile(string folderPath, string fileName, IFormFile formFile)
    {
        var connectionInfo = GetConnectionInfo();
        using var sftpClient = new SftpClient(connectionInfo);

        sftpClient.Connect();
        if (!sftpClient.IsConnected) return;

        CreateDirectoryIfNotExists(sftpClient, folderPath);
        sftpClient.ChangeDirectory(folderPath);

        using (var ms = new MemoryStream())
        {
            formFile.CopyTo(ms);
            var fileBytes = ms.ToArray();

            var filePath = Path.Combine(folderPath, fileName);
            sftpClient.WriteAllBytes(filePath, fileBytes);
        }

        sftpClient.Disconnect();
    }

    public void DeleteFile(string folderPath, string fileName)
    {
        var connectionInfo = GetConnectionInfo();
        using var sftpClient = new SftpClient(connectionInfo);

        sftpClient.Connect();
        if (!sftpClient.IsConnected) return;

        sftpClient.ChangeDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        if (sftpClient.Exists(filePath)) sftpClient.Delete(filePath);
    }

    public byte[] ReadFile(string folderPath, string fileName)
    {
        var fileBytes = Array.Empty<byte>();

        var connectionInfo = GetConnectionInfo();
        using var sftpClient = new SftpClient(connectionInfo);

        sftpClient.Connect();
        if (!sftpClient.IsConnected) return fileBytes;

        sftpClient.ChangeDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        fileBytes = sftpClient.ReadAllBytes(filePath);

        return fileBytes;
    }

    private ConnectionInfo GetConnectionInfo()
    {
        var authMethod = new PasswordAuthenticationMethod
        (
            configSettings.SftpSettings.UserName,
            configSettings.SftpSettings.Password
        );

        return new ConnectionInfo
        (
            configSettings.SftpSettings.Ip,
            configSettings.SftpSettings.UserName,
            authMethod
        );
    }

    private static void CreateDirectoryIfNotExists(ISftpClient client, string folderPath)
    {
        if (!client.Exists(folderPath))
            client.CreateDirectory(folderPath);
    }
}