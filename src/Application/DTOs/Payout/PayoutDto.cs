using Application.Shared.Enum;
using AutoMapper;
using Domain.Payment;

public class PayoutDto
{
    public Guid Id { get; set; }
    public Guid MuseumId { get; set; }
    public Guid BankAccountId { get; set; }
    public float Amount { get; set; }
    public DateTime ProcessedDate { get; set; }
    public PayoutStatusEnum Status { get; set; }
}
public class PayoutProfile : Profile
{
    public PayoutProfile()
    {
        CreateMap<Payout, PayoutDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<PayoutReq, Payout>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}