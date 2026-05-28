using API.Attributes;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Helpers;
using CORE.Localization;
using DTO.File;
using DTO.Responses;
using ENTITIES.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using IResult = DTO.Responses.IResult;
using Path = System.IO.Path;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class FileController(
    IFileService fileService,
    ISftpService sftpService) : ControllerBase
{
    // Per-type upload policy: which extensions are accepted and max byte size.
    // Adding a new FileType requires adding an entry here AND an IFileTypeHandler.
    private static readonly Dictionary<FileType, (Func<IFormFile, bool> Validator, long MaxBytes, string FriendlyLimit)>
        UploadPolicy = new()
        {
            [FileType.UserProfile] = (FileHelper.IsValidImage, 2 * 1024 * 1024, "2MB"),
            [FileType.OrganizationLogo] = (FileHelper.IsValidImage, 2 * 1024 * 1024, "2MB")
        };

    [SwaggerOperation(Summary = "upload file")]
    [Produces(typeof(IDataResult<string>))]
    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] FileUploadRequestDto dto)
    {
        if (dto.File is null || dto.File.Length == 0)
            return BadRequest(new ErrorResult(Messages.FileIsNotFound.Translate()));

        if (!UploadPolicy.TryGetValue(dto.Type, out var policy))
            return BadRequest(new ErrorResult(Messages.ThisFileTypeIsNotAllowed.Translate()));

        if (!policy.Validator(dto.File))
            return BadRequest(new ErrorResult(Messages.ThisFileTypeIsNotAllowed.Translate()));

        if (dto.File.Length > policy.MaxBytes)
            return BadRequest(new ErrorResult(
                Messages.FileIsLargeThan2Mb.Translate().Replace("{value}", policy.FriendlyLimit)));

        var originalFileName = Path.GetFileName(dto.File.FileName);
        var hashFileName = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(dto.File.FileName);
        var path = dto.Type.ToString();

        // Images don't need PDF JavaScript stripping. When a future FileType maps to PDF,
        // call FileHelper.RemoveJavaScriptFromPdfAsync(dto.File) before upload.
        await sftpService.UploadFileAsync(path, $"{hashFileName}{fileExtension}", dto.File);

        var fileToAdd = new FileToAddDto(
            originalFileName, hashFileName, fileExtension, dto.File.Length, path, dto.Type);
        await fileService.AddAsync(fileToAdd, dto);

        return Ok(new SuccessDataResult<string>(hashFileName, Messages.Success.Translate()));
    }

    [SwaggerOperation(Summary = "delete file")]
    [Produces(typeof(IResult))]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] FileRemoveRequestDto dto)
    {
        var fileResult = await fileService.GetAsync(dto.HashName);
        if (!fileResult.Success) return BadRequest(fileResult);

        await sftpService.DeleteFileAsync(fileResult.Data!.Path!,
            $"{fileResult.Data.HashName}{fileResult.Data.Extension}");

        var result = await fileService.RemoveAsync(dto);
        return Ok(result);
    }

    [SwaggerOperation(Summary = "download file")]
    [Produces(typeof(void))]
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string hashName)
    {
        var fileResult = await fileService.GetAsync(hashName);
        if (!fileResult.Success) return BadRequest(fileResult);

        var fileName = $"{fileResult.Data!.HashName}{fileResult.Data.Extension}";
        var fileData = await sftpService.ReadFileAsync(fileResult.Data!.Path!, fileName);

        return File(fileData, "application/octet-stream", fileName);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "get file")]
    [Produces(typeof(void))]
    public async Task<IActionResult> Get([FromQuery] string hashName)
    {
        var fileResult = await fileService.GetAsync(hashName);
        if (!fileResult.Success) return BadRequest(fileResult);

        var fileName = $"{fileResult.Data!.HashName}{fileResult.Data.Extension}";
        var fileStream = await sftpService.ReadFileAsync(fileResult.Data!.Path!, fileName);

        if (fileStream is null) return BadRequest(new ErrorResult(Messages.FileIsNotFound.Translate()));

        var contentType = fileResult.Data.Extension?.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        return File(fileStream, contentType);
    }
}