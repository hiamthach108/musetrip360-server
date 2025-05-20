using Application.DTOs.Pagination;

public class TourContentQuery : PaginationReq
{
    public Guid? TourId { get; set; }
    public string? SearchKeyword { get; set; }
}

