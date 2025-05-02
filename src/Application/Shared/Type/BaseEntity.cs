namespace Application.Shared.Type;

using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;


public class BaseEntity
{
  public Guid Id { get; set; }

  [Column(TypeName = "jsonb")]
  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}