using System.Text.Json;
using AutoMapper;
using Domain.Tours;

public class TourContentDto
{
    public Guid Id { get; set; }
    public Guid TourId { get; set; }
    public string Content { get; set; } = null!;
    public bool IsActive { get; set; }
    public int ZOrder { get; set; }
    public JsonDocument? JsonContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TourContentProfile : Profile
{
    public TourContentProfile()
    {
        CreateMap<TourContent, TourContentDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourContentCreateDto, TourContent>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<TourContentUpdateDto, TourContent>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
