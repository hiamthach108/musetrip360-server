namespace Application.Shared.Constant;

public static class FileConst
{
  public const string IMAGE = "image";
  public const string VIDEO = "video";
  public const string DOCUMENT = "document";

  public const int MAX_IMAGE_SIZE = 5 * 1024 * 1024; // 5MB
  public const int MAX_VIDEO_SIZE = 500 * 1024 * 1024; // 500MB
  public const int MAX_DOCUMENT_SIZE = 5 * 1024 * 1024; // 5MB

  public static readonly string[] IMAGE_CONTENT_TYPES = { "image/jpeg", "image/png", "image/gif", "image/webp", "image/jpg", "image/svg+xml", "image/bmp", "image/tiff", "image/x-icon", "image/heic", "image/heif", "image/avif" };
  public static readonly string[] VIDEO_CONTENT_TYPES = { "video/mp4", "video/avi", "video/quicktime", "video/x-ms-wmv", "video/x-flv", "video/x-matroska", "video/quicktime", "video/webm" };
  public static readonly string[] DOCUMENT_CONTENT_TYPES = { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.ms-powerpoint", "application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/zip", "application/x-rar-compressed", "application/x-7z-compressed", "application/x-tar", "application/x-gzip", "application/x-bzip2", "application/x-xz", "application/x-rar", "application/x-7z", "application/x-tar", "application/x-gzip", "application/x-bzip2", "application/x-xz" };
}

