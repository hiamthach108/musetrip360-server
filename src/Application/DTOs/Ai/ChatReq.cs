namespace Application.DTOs.Ai;

public class ChatReq
{
  public string Prompt { get; set; } = "";
  public bool? IsVector { get; set; } = false;
  public string? EntityType { get; set; }
}

public class UploadFromBase64Req
{
  public string Base64 { get; set; } = "";
  public string MimeType { get; set; } = "";
}