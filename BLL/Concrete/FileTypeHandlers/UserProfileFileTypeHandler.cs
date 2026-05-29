using BLL.Abstract;
using DTO.File;
using ENTITIES.Enums;

namespace BLL.Concrete.FileTypeHandlers;

/// <summary>
///     <see cref="IFileTypeHandler" /> for <c>FileType.UserProfile</c> uploads. Links the newly
///     stored file to the user identified in the upload request on add; clears the profile-picture
///     association (sets <c>ProfileFileId = null</c>) on remove.
/// </summary>
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