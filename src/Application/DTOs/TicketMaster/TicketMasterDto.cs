using System.Text.Json;
using AutoMapper;
using Domain.Tickets;

public class TicketMasterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public float Price { get; set; }
    public float DiscountPercentage { get; set; }
    public int GroupSize { get; set; }
    public bool IsActive { get; set; }
    public Guid MuseumId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public JsonDocument? Metadata { get; set; }
}
public class TicketMasterProfile : Profile
{
    public TicketMasterProfile()
    {
        CreateMap<TicketMaster, TicketMasterDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TicketMasterCreateDto, TicketMaster>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TicketMasterUpdateDto, TicketMaster>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}