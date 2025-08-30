using System.ComponentModel.DataAnnotations;
using Application.DTOs.Pagination;
using Application.Shared.Enum;

public class PayoutQuery : PaginationReq
{
    public Guid? MuseumId { get; set; }
    public Guid? BankAccountId { get; set; }
    [Range(0, float.MaxValue)]
    public float? AmountFrom { get; set; } = 0;
    [Range(0, float.MaxValue)]
    public float? AmountTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public PayoutStatusEnum? Status { get; set; }
}