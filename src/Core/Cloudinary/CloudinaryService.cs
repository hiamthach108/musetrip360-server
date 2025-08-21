namespace Core.Cloudinary;

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public interface ICloudinaryService
{
  Task<string> UploadFileAsync(IFormFile file);
  Task<bool> DeleteImageAsync(string publicId);
  Task<string?> UploadFromBase64Async(string base64, string mimeType);
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
    await file.CopyToAsync(ms);
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

  public async Task<string?> UploadFromBase64Async(string base64, string mimeType)
  {
    try
    {
      // Remove data URL prefix if present (e.g., "data:audio/L16;codec=pcm;rate=24000;base64,")
      if (base64.Contains(','))
      {
        base64 = base64.Split(',')[1];
      }

      // Convert base64 to byte array
      var fileBytes = Convert.FromBase64String(base64);

      // Check if it's an unsupported audio format that needs conversion
      if (IsUnsupportedAudioFormat(mimeType))
      {
        // Convert PCM to WAV format
        var wavBytes = ConvertPcmToWav(fileBytes, mimeType);
        fileBytes = wavBytes;
        mimeType = "audio/wav"; // Update MIME type after conversion
      }

      // Determine file extension from MIME type
      var fileExtension = GetFileExtensionFromMimeType(mimeType);
      var fileName = $"{Guid.NewGuid()}{fileExtension}";

      // Check if it's an audio file
      if (IsAudioMimeType(mimeType))
      {
        await using var ms = new MemoryStream(fileBytes);
        var uploadParams = new VideoUploadParams()
        {
          File = new FileDescription(fileName, ms),
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        // Check if upload was successful
        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
          return uploadResult.SecureUrl.AbsoluteUri;
        }
      }
      else
      {
        // Handle image files
        await using var ms = new MemoryStream(fileBytes);
        var uploadParams = new ImageUploadParams()
        {
          File = new FileDescription(fileName, ms),
          // Optional: Add transformation parameters for images
          Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        // Check if upload was successful
        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
          return uploadResult.SecureUrl.AbsoluteUri;
        }
      }

      return null;
    }
    catch (Exception ex)
    {
      // Log the exception (consider using ILogger)
      // _logger.LogError(ex, "Failed to upload base64 file to Cloudinary: {Error}", ex.Message);
      return null;
    }
  }

  private static string GetFileExtensionFromMimeType(string mimeType)
  {
    // Extract base MIME type (remove parameters like codec, rate, etc.)
    var baseMimeType = mimeType?.Split(';')[0].ToLower();

    return baseMimeType switch
    {
      // Supported audio formats by Cloudinary
      "audio/wav" => ".wav",
      "audio/wave" => ".wav",
      "audio/x-wav" => ".wav",
      "audio/mpeg" => ".mp3",
      "audio/mp3" => ".mp3",
      "audio/mp4" => ".m4a",
      "audio/aac" => ".aac",
      "audio/ogg" => ".ogg",
      "audio/webm" => ".webm",
      "audio/flac" => ".flac",
      "audio/x-flac" => ".flac",

      // Image formats
      "image/jpeg" or "image/jpg" => ".jpg",
      "image/png" => ".png",
      "image/gif" => ".gif",
      "image/webp" => ".webp",
      "image/svg+xml" => ".svg",
      "image/bmp" => ".bmp",
      "image/tiff" => ".tiff",

      // Default fallback based on the primary type
      _ when baseMimeType?.StartsWith("audio/") == true => ".wav",
      _ => ".jpg" // Default fallback for images
    };
  }

  private static bool IsAudioMimeType(string mimeType)
  {
    var baseMimeType = mimeType?.Split(';')[0].ToLower();
    return baseMimeType?.StartsWith("audio/") == true;
  }

  private static bool IsUnsupportedAudioFormat(string mimeType)
  {
    var baseMimeType = mimeType?.Split(';')[0].ToLower();

    // List of unsupported raw audio formats that need conversion
    return baseMimeType switch
    {
      "audio/l16" => true,
      "audio/pcm" => true,
      "audio/x-pcm" => true,
      "audio/raw" => true,
      _ => false
    };
  }

  private static byte[] ConvertPcmToWav(byte[] pcmData, string mimeType)
  {
    // Parse sample rate from MIME type (default to 24000 if not found)
    var sampleRate = 24000;
    var bitsPerSample = 16;

    if (mimeType.Contains("rate="))
    {
      var rateMatch = System.Text.RegularExpressions.Regex.Match(mimeType, @"rate=(\d+)");
      if (rateMatch.Success && int.TryParse(rateMatch.Groups[1].Value, out var parsedRate))
      {
        sampleRate = parsedRate;
      }
    }

    // Determine bits per sample (L16 = 16-bit)
    if (mimeType.Contains("L16"))
    {
      bitsPerSample = 16;
    }

    var channels = 1; // Assuming mono audio
    var bytesPerSample = bitsPerSample / 8;
    var blockAlign = channels * bytesPerSample;
    var byteRate = sampleRate * blockAlign;

    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms);

    // WAV Header
    writer.Write("RIFF".ToCharArray());
    writer.Write((uint)(36 + pcmData.Length)); // File size - 8
    writer.Write("WAVE".ToCharArray());

    // Format chunk
    writer.Write("fmt ".ToCharArray());
    writer.Write((uint)16); // Chunk size
    writer.Write((ushort)1); // Audio format (PCM)
    writer.Write((ushort)channels);
    writer.Write((uint)sampleRate);
    writer.Write((uint)byteRate);
    writer.Write((ushort)blockAlign);
    writer.Write((ushort)bitsPerSample);

    // Data chunk
    writer.Write("data".ToCharArray());
    writer.Write((uint)pcmData.Length);
    writer.Write(pcmData);

    return ms.ToArray();
  }
}