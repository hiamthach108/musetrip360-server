using System.Text.Json;
using Application.Shared.Enum;
using AutoMapper;

public class RoomDto
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public RoomStatusEnum Status { get; set; }
    public Guid EventId { get; set; }
    public JsonDocument? Metadata { get; set; }
}
public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<RoomCreateDto, Room>()
            .ConstructUsing((src, ctx) => new Room(Guid.Empty, src.Name, src.Status, src.Description))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<RoomUpdateDto, Room>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<RoomUpdateMetadataDto, Room>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}