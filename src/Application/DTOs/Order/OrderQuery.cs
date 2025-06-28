namespace Application.DTOs.Order;

using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class OrderQuery : PaginationReq
{
  public string? Search { get; set; }
  public string? Sort { get; set; }
  public string? SortBy { get; set; }
  public OrderTypeEnum? OrderType { get; set; }
  public PaymentStatusEnum? Status { get; set; }
}