using API.Attributes;
using BLL.Abstract;
using CORE.Abstract;
using CORE.Constants;
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

[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ValidateToken]
public class FileController : Controller
{
    private readonly IFileService _fileService;
    private readonly IUtilService _utilService;
    private readonly ISftpService _sftpService;

    public FileController(
        IFileService fileService,
        IUtilService utilService,
        ISftpService sftpService
    )
    {
        _fileService = fileService;
        _utilService = utilService;
        _sftpService = sftpService;
    }

    [SwaggerOperation(Summary = "upload file")]
    [Produces(typeof(IDataResult<string>))]
    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] FileUploadRequestDto dto)
    {
        // create file
        var originalFileName = Path.GetFileName(dto.File.FileName);
        var hashFileName = Guid.NewGuid().ToString();
        var fileExtension = Path.GetExtension(dto.File.FileName);

        // check extension
        if (!Constants.AllowedFileExtensions.Contains(fileExtension))
            return BadRequest(new ErrorDataResult<string>(message: Messages.ThisFileTypeIsNotAllowed.Translate()));

        var path = dto.Type.ToString();
        _sftpService.UploadFile(path, $"{hashFileName}{fileExtension}", dto.File);

        // or
        // var path = _utilService.GetEnvFolderPath(dto.Type.ToString());
        // await FileHelper.WriteFile(dto.File, $"{hashFileName}{fileExtension}", path);

        // add to database
        var fileToAdd = new FileToAddDto(originalFileName, hashFileName, fileExtension, dto.File.Length, path, dto.Type);
        await _fileService.AddAsync(fileToAdd, dto);

        return Ok(new SuccessDataResult<string>(hashFileName, Messages.Success.Translate()));
    }

    [SwaggerOperation(Summary = "delete file")]
    [Produces(typeof(IResult))]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] FileRemoveRequestDto dto)
    {
        // delete file
        var fileResult = await _fileService.GetAsync(dto.HashName);
        if (fileResult.Success) return BadRequest(fileResult);

        _sftpService.DeleteFile(fileResult.Data!.Path!, $"{fileResult.Data.HashName}{fileResult.Data.Extension}");

        // or
        // var path = Path.Combine(_utilService.GetEnvFolderPath(dto.Type.ToString()), dto.HashName);
        // FileHelper.DeleteFile(path);

        // remove from database
        var result = await _fileService.RemoveAsync(dto);

        return Ok(result);
    }


    [SwaggerOperation(Summary = "download file")]
    [Produces(typeof(void))]
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string hashName, [FromQuery] FileType type)
    {
        // get file from database
        var fileResult = await _fileService.GetAsync(hashName);
        if(fileResult.Success) return BadRequest(fileResult);

        // read file as stream
        var fileName = $"{fileResult.Data!.HashName}{fileResult.Data.Extension}";
        var fileData = _sftpService.ReadFile(fileResult.Data!.Path!, fileName);

        return File(fileData, "APPLICATION/octet-stream", fileName);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "get file")]
    [Produces(typeof(void))]
    public async Task<IActionResult> Get([FromQuery] string hashName, [FromQuery] FileType type)
    {
        // get file from database
        var fileResult = await _fileService.GetAsync(hashName);

        // read file as stream
        var fileStream = _sftpService.ReadFile(fileResult.Data!.Path!, $"{fileResult.Data.HashName}{fileResult.Data.Extension}");

        // or
        // var path = Path.Combine(_utilService.GetEnvFolderPath(_utilService.GetFolderName(type)), $"{hashName}{file.Data!.Extension}");
        // var fileStream = System.IO.File.OpenRead(path);

        if (fileStream is null) return BadRequest(new ErrorResult(Messages.FileIsNotFound.Translate()));

        return File(fileStream, "image/png");
    }
}