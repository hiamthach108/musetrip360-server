namespace Application.DTOs.Pagination;

public class PaginationReq
{
  public int Page { get; set; }
  public int PageSize { get; set; }
}

public class PaginationResp
{
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int Total { get; set; }
}
