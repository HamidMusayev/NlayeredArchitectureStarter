using ENTITIES.Enums;
using ENTITIES.Identifiers;
using Microsoft.AspNetCore.Http;

namespace DTO.File;

/// <summary>Outbound metadata for a stored file. The actual bytes live in <c>IBlobStorage</c>.</summary>
public record FileToListDto(
    Guid Id,
    string OriginalName,
    string HashName,
    string Extension,
    double Length,
    string? Path,
    FileType Type
);

/// <summary>Persistence-layer payload assembled by the controller after the upload is staged in blob storage.</summary>
public record FileToAddDto(
    string OriginalName,
    string HashName,
    string Extension,
    double Length,
    string? Path,
    FileType Type
);

/// <summary>
///     Inbound multipart upload request. Validated by <c>FileUploadRequestDtoValidator</c>
///     against the per-<see cref="Type" /> upload policy (allowed extensions + size cap)
///     before reaching the action body.
/// </summary>
public record FileUploadRequestDto(
    IFormFile? File,
    FileType Type,
    Guid? UserId,
    OrganizationId? OrganizationId
);

/// <summary>Inbound payload for deleting a previously uploaded file by its opaque hashed name.</summary>
public record FileRemoveRequestDto(
    string HashName,
    FileType Type,
    Guid? UserId,
    OrganizationId? OrganizationId
);