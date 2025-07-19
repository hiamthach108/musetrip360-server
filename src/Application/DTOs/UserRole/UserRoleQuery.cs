namespace Application.DTOs.UserRole;

using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class UserRoleQuery : PaginationReq
{
  public string? Search { get; set; }
  public string? MuseumId { get; set; }
  public List<UserStatusEnum> Statuses { get; set; } = [];
}