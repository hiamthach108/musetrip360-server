namespace Domain.Content;

using Application.Shared.Type;
using Domain.Museums;

public class Category : BaseEntity
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }

  public ICollection<Museum> Museums { get; set; } = new List<Museum>();
}