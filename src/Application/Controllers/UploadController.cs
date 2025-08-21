namespace Application.Controllers;

using Application.DTOs.Ai;
using Application.Middlewares;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Application.Shared.Type;
using Core.Cloudinary;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/upload")]
public class UploadController : ControllerBase
{
  private readonly ILogger<UploadController> _logger;

  private readonly ICloudinaryService _uploadService;

  public UploadController(ILogger<UploadController> logger, ICloudinaryService uploadService)
  {
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _uploadService = uploadService;
  }

  [Protected]
  [HttpPost]
  public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] MediaTypeEnum type)
  {
    if (file == null || file.Length == 0)
    {
      return ErrorResp.BadRequest("File is empty");
    }
    _logger.LogInformation($"File name: {file.FileName}, File size: {file.Length}");

    // check file type is image
    var validationResult = type switch
    {
      MediaTypeEnum.Image => ValidateFile(
          file,
          FileConst.IMAGE_CONTENT_TYPES,
          FileConst.MAX_IMAGE_SIZE,
          "File is not image type (jpg, png, gif, webp)",
          "File is too large, max size is 5MB"
      ),

      MediaTypeEnum.Video => ValidateFile(
          file,
          FileConst.VIDEO_CONTENT_TYPES,
          FileConst.MAX_VIDEO_SIZE,
          "File is not video type (mp4, avi, mov, wmv, flv, mkv, wav)",
          "File is too large, max size is 20MB"
      ),

      MediaTypeEnum.Document => ValidateFile(
          file,
          FileConst.DOCUMENT_CONTENT_TYPES,
          FileConst.MAX_DOCUMENT_SIZE,
          "File is not Document type (pdf, doc, docx)",
          "File is too large max size is 5MB"
      ),

      _ => ErrorResp.BadRequest("File type is invalid")
    };

    if (validationResult != null)
    {
      return validationResult;
    }

    _logger.LogInformation($"Start upload file: {file.FileName}");

    var url = await _uploadService.UploadFileAsync(file);

    _logger.LogInformation($"Upload file success: {file.FileName}");

    return SuccessResp.Ok(new
    {
      url
    });
  }

  private IActionResult? ValidateFile(
    IFormFile file,
    string[] allowedTypes,
    long maxSize,
    string typeErrorMessage,
    string sizeErrorMessage)
  {
    var contentType = file.ContentType;

    // if (!allowedTypes.Contains(contentType))
    // {
    //   return ErrorResp.BadRequest(typeErrorMessage);
    // }

    if (file.Length > maxSize)
    {
      return ErrorResp.BadRequest(sizeErrorMessage);
    }

    return null;
  }

  [Protected]
  [HttpPost("base64")]
  public async Task<IActionResult> UploadFromBase64Async([FromBody] UploadFromBase64Req request)
  {
    if (string.IsNullOrEmpty(request.Base64))
    {
      return ErrorResp.BadRequest("Base64 string is empty");
    }

    var url = await _uploadService.UploadFromBase64Async(request.Base64, request.MimeType);
    if (string.IsNullOrEmpty(url))
    {
      return ErrorResp.InternalServerError("Upload from base64 failed");
    }

    return SuccessResp.Ok(new
    {
      url
    });
  }
}