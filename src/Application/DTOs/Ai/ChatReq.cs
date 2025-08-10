namespace Application.DTOs.Ai;

public class ChatReq
{
  public string Prompt { get; set; } = "";
  public bool? IsVector { get; set; } = false;
}