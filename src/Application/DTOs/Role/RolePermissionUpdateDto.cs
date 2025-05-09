namespace Application.DTOs.Role;

public class RolePermissionUpdateDto
{
  public ICollection<Guid> AddList { get; set; } = new List<Guid>();
  public ICollection<Guid> RemoveList { get; set; } = new List<Guid>();
}