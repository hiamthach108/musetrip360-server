using Application.DTOs.Pagination;

public class TourOnlineQuery : PaginationReq
{
    public Guid? MuseumId { get; set; }
    public string? SearchKeyword { get; set; }
}

