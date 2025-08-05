namespace Domain.Reviews;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Museums;
using Domain.Users;

public class Feedback : BaseEntity
{
  public string Comment { get; set; } = null!;
  public int Rating { get; set; }
  public Guid TargetId { get; set; }
  public DataEntityType Type { get; set; }
  public Guid CreatedBy { get; set; }
  public User CreatedByUser { get; set; } = null!;
}