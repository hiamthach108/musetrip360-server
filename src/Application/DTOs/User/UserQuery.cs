namespace Application.DTOs.User;

using Application.DTOs.Pagination;

public class UserQuery : PaginationReq
{
  public string? SearchKeyword { get; set; }
}