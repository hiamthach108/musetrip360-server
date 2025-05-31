using Application.DTOs.Pagination;
using Application.Shared.Type;

public class TourGuideQuery : PaginationReq
{
  public string? Name { get; set; }
  public string? Bio { get; set; }
  public bool? IsAvailable { get; set; }
  public Guid? MuseumId { get; set; }
  public Guid? UserId { get; set; }
  public Guid? EventId { get; set; }
}