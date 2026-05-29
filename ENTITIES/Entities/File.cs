using ENTITIES.Entities.Generic;
using ENTITIES.Enums;

namespace ENTITIES.Entities;

/// <summary>
///     Uploaded file metadata row. The actual bytes live in the configured
///     <see cref="BLOBSTORAGE.Abstract.IBlobStorage" /> backend (filesystem / SFTP / S3) under the
///     container derived from <see cref="Type" />; this row tracks the original filename, the
///     opaque hashed name used in the store, and basic mime metadata.
/// </summary>
public class File : Auditable, IEntity
{
    public required string OriginalName { get; set; }
    public required string HashName { get; set; }
    public required string Extension { get; set; }
    public required double Length { get; set; }
    public required string Path { get; set; }
    public required FileType Type { get; set; }
}