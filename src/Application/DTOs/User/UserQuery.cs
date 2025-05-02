using Application.DTOs.Pagination;

namespace Application.DTOs.User;

public class UserQuery : PaginationReq
{
  public string? SearchKeyword { get; set; }
}