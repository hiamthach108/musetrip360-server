using Application.DTOs.Pagination;

public class TourOnlineQuery : PaginationReq
{
    public Guid? MuseumId { get; set; }
    public string? Search { get; set; }
    public List<Guid>? Ids { get; set; }
}

