namespace Core.Cloudinary;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public interface ICloudinaryService
{
  Task<string> UploadFileAsync(IFormFile file);
  Task<bool> DeleteImageAsync(string publicId);
}

public class CloudinaryService : ICloudinaryService
{
  private readonly Cloudinary _cloudinary;
  public CloudinaryService(IConfiguration configuration)
  {
    Account account = new Account(
      configuration["Cloudinary:CloudName"],
      configuration["Cloudinary:ApiKey"],
      configuration["Cloudinary:ApiSecret"]
    );
    _cloudinary = new Cloudinary(account);
  }

  public async Task<string> UploadFileAsync(IFormFile file)
  {
    await using var ms = new MemoryStream();
    file.CopyTo(ms);
    var fileBytes = ms.ToArray();
    var uploadParams = new ImageUploadParams()
    {
      File = new FileDescription(file.FileName, new MemoryStream(fileBytes)),
    };
    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
    return uploadResult.SecureUrl.AbsoluteUri;
  }

  public async Task<bool> DeleteImageAsync(string publicId)
  {
    var deleteParams = new DeletionParams(publicId);
    var result = await _cloudinary.DestroyAsync(deleteParams);
    return result.Result == "ok";
  }
}