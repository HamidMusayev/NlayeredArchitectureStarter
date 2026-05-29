using AutoMapper;
using DTO.File;
using File = ENTITIES.Entities.File;

namespace BLL.Mappers;

/// <summary>
///     AutoMapper profile for <c>File</c> entity ↔ DTO conversions.
///     Maps <c>FileToAddDto → File</c> (inbound) and <c>File → FileToListDto</c> (outbound).
/// </summary>
public class FileMapper : Profile
{
    public FileMapper()
    {
        CreateMap<FileToAddDto, File>();
        CreateMap<File, FileToListDto>();
    }
}