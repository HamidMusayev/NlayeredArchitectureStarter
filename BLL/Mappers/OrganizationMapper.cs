using DTO.File;
using DTO.Organization;
using ENTITIES.Entities;
using Riok.Mapperly.Abstractions;
using File = ENTITIES.Entities.File;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>Organization</c> ↔ DTO conversions (Mapperly source generator).
///     Outbound <see cref="ToListDto" /> recursively projects the <c>Parent</c> tree (Mapperly
///     auto-handles the self-referential <c>OrganizationToListDto.Parent</c>) and the
///     <c>LogoFile</c> nested type via the local <see cref="ToFileListDto" /> helper.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class OrganizationMapper
{
    public partial OrganizationToListDto ToListDto(Organization source);
    public partial List<OrganizationToListDto> ToListDtos(IEnumerable<Organization> source);

    public partial void UpdateEntity(OrganizationToAddDto source, Organization target);
    public partial void UpdateEntity(OrganizationToUpdateDto source, Organization target);

    // Nested helper used by ToListDto.LogoFile. Drop when FileMapper composition lands.
    private partial FileToListDto? ToFileListDto(File? source);
}