namespace Application.DTOs.User;

using System.Text.Json;

public class UpdateProfileReq
{
  public string? FullName { get; set; }
  public string? PhoneNumber { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime? BirthDate { get; set; }
  public JsonDocument? Metadata { get; set; }
}