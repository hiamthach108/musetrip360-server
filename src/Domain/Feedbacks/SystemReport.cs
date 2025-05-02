namespace Domain.Reviews;

using Application.Shared.Enum;
using Application.Shared.Type;
using Domain.Users;

public class SystemReport : BaseEntity
{
  public string Title { get; set; } = null!;
  public string Description { get; set; } = null!;
  public Guid CreatedBy { get; set; }
  public ReportStatusEnum Status { get; set; }

  public User CreatedByUser { get; set; } = null!;
}
