namespace ENTITIES.Enums;

/// <summary>
///     Logical category of an uploaded file. Each value maps 1:1 to an
///     <c>IFileTypeHandler</c> implementation (post-upload side effects) and to a blob
///     container in <see cref="BLOBSTORAGE.Abstract.IBlobStorage" />.
/// </summary>
public enum FileType
{
    UserProfile,
    OrganizationLogo
}