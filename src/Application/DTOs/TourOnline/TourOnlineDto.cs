using System.Text.Json;
using AutoMapper;
using Domain.Tours;

public class TourOnlineDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
    public float Price { get; set; }
    public Guid MuseumId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<TourContentDto> TourContents { get; set; } = null!;
}
public class TourOnlineProfile : Profile
{
    public TourOnlineProfile()
    {
        CreateMap<TourOnline, TourOnlineDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourOnlineCreateDto, TourOnline>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourOnlineUpdateDto, TourOnline>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}

