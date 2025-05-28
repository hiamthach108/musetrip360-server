using System.Text.Json;
using AutoMapper;
using Domain.Tours;

public class TourGuideDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Bio { get; set; } = null!;
    public bool IsAvailable { get; set; }
    public Guid MuseumId { get; set; }
    public Guid UserId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class TourGuideProfile : Profile
{
    public TourGuideProfile()
    {
        CreateMap<TourGuide, TourGuideDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourGuideCreateDto, TourGuide>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourGuideUpdateDto, TourGuide>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}