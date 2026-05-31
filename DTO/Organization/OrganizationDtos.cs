using DTO.File;
using ENTITIES.Identifiers;

namespace DTO.Organization;

/// <summary>Inbound payload for creating an organization. <see cref="ParentId" /> chains it into a tree.</summary>
public record OrganizationToAddDto(
    string FullName,
    string ShortName,
    string Address,
    OrganizationId? ParentId,
    string PhoneNumber,
    string Tin,
    string Email,
    string Rekvizit
);

/// <summary>Outbound organization with nested parent (recursive) and logo file.</summary>
public record OrganizationToListDto(
    Guid Id,
    string FullName,
    string ShortName,
    string Address,
    OrganizationToListDto? Parent,
    string PhoneNumber,
    string Tin,
    string Email,
    string Rekvizit,
    FileToListDto? LogoFile
);

/// <summary>Inbound payload for editing an organization.</summary>
public record OrganizationToUpdateDto(
    string FullName,
    string ShortName,
    string Address,
    OrganizationId? ParentId,
    string PhoneNumber,
    string Tin,
    string Email,
    string Rekvizit
);