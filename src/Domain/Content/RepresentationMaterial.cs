namespace Domain.Content;

using Application.Shared.Type;
using Domain.Events;
using Domain.Users;

public class RepresentationMaterial : BaseEntity
{
  public Guid EventId { get; set; }
  public string Content { get; set; } = string.Empty;
  public int ZOrder { get; set; }
  public Guid CreatedBy { get; set; }

  public Event Event { get; set; } = new();
  public User CreatedByUser { get; set; } = new();
}