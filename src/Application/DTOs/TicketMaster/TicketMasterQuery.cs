using Application.DTOs.Pagination;

public class TicketMasterQuery : PaginationReq
{
    public string? SearchKeyword { get; set; }
    public float? Price { get; set; }
    public float? DiscountPercentage { get; set; }
    public Guid? MuseumId { get; set; }
    public int? GroupSize { get; set; }
}