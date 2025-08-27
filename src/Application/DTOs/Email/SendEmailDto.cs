namespace Application.DTOs.Email;

public class SendEmailDto
{
  public string Type { get; set; } = string.Empty;
  public List<string> Recipients { get; set; } = new();
  public string Subject { get; set; } = string.Empty;
  public Dictionary<string, object> TemplateData { get; set; } = new();
}