namespace Domain.Museums;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class MuseumPolicy : BaseEntity
{
  public string Title { get; set; } = null!;
  public string Content { get; set; } = null!;
  public PolicyTypeEnum PolicyType { get; set; }
  public bool IsActive { get; set; }
  public int ZOrder { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }

  public Museum Museum { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;
}