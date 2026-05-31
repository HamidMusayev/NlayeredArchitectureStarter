using DTO.File;
using Riok.Mapperly.Abstractions;
using File = ENTITIES.Entities.File;

namespace BLL.Mappers;

/// <summary>
///     Compile-time mapper for <c>File</c> ↔ DTO conversions (Mapperly source generator).
///     <see cref="UpdateEntity" /> writes to a caller-constructed target so audit fields the
///     DTO doesn't carry stay untouched.
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class FileMapper
{
    public partial FileToListDto ToListDto(File source);
    public partial List<FileToListDto> ToListDtos(IEnumerable<File> source);

    public partial void UpdateEntity(FileToAddDto source, File target);
}