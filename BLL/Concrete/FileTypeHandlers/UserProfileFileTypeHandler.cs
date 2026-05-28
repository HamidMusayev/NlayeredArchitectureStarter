using BLL.Abstract;
using DTO.File;
using ENTITIES.Enums;

namespace BLL.Concrete.FileTypeHandlers;

public class UserProfileFileTypeHandler(IUserService userService) : IFileTypeHandler
{
    public FileType Type => FileType.UserProfile;

    public async Task OnFileAddedAsync(Guid fileId, FileUploadRequestDto request)
    {
        await userService.AddProfileAsync(request.UserId!.Value, fileId);
    }

    public async Task OnFileRemovedAsync(FileRemoveRequestDto request)
    {
        await userService.AddProfileAsync(request.UserId!.Value, null);
    }
}