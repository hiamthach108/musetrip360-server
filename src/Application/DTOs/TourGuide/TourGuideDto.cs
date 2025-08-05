using System.Text.Json;
using Application.DTOs.User;
using AutoMapper;
using Domain.Tours;
using Domain.Users;

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

    public UserDto User { get; set; } = null!;
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