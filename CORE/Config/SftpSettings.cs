namespace CORE.Config;

/// <summary>SFTP credentials used by <c>SftpService</c> + <c>SftpBlobStorage</c>.</summary>
public record SftpSettings
{
    public required string UserName { get; set; }
    public required string Ip { get; set; }
    public required string Password { get; set; }
}