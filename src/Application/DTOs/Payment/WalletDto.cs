namespace Application.DTOs.Payment;

using AutoMapper;
using Domain.Payment;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid MuseumId { get; set; }
    public float AvailableBalance { get; set; }
    public float PendingBalance { get; set; }
    public float TotalBalance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WalletProfile : Profile
{
    public WalletProfile()
    {
        CreateMap<MuseumWallet, WalletDto>();
    }
}
