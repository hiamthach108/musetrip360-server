namespace Domain.Museums;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class Article : BaseEntity
{
  public string Title { get; set; } = null!;
  public string Content { get; set; } = null!;
  public ArticleStatusEnum Status { get; set; }
  public DateTime PublishedAt { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }
  public DataEntityType DataEntityType { get; set; }
  public Guid EntityId { get; set; }

  public Museum Museum { get; set; } = null!;
  public User CreatedByUser { get; set; } = null!;
}