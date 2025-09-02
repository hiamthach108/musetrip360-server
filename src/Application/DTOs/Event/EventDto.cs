using System.Text.Json;
using Application.DTOs.RepresentationMaterial;
using Application.DTOs.User;
using Application.Shared.Enum;
using AutoMapper;
using Domain.Events;
using MuseTrip360.src.Application.DTOs.Artifact;

public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventTypeEnum EventType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = null!;
    public int Capacity { get; set; }
    public int AvailableSlots { get; set; }
    public DateTime BookingDeadline { get; set; }
    public Guid MuseumId { get; set; }
    public Guid CreatedBy { get; set; }
    public EventStatusEnum Status { get; set; }
    public float Price { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public UserDto CreatedByUser { get; set; } = null!;


    public ICollection<ArtifactDto>? Artifacts { get; set; }
    public ICollection<TourOnlineDto>? TourOnlines { get; set; }
    public ICollection<TourGuideDto>? TourGuides { get; set; }
    public ICollection<RepresentationMaterialDto>? RepresentationMaterials { get; set; }
    public ICollection<EventParticipantNoNavigateDto>? EventParticipants { get; set; }
    public ICollection<RoomDto>? Rooms { get; set; }
    //   public ICollection<TicketAddonDto> TicketAddons { get; set; } = null!;
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Event, EventDto>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EventCreateDto, Event>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EventUpdateDto, Event>()
            .ForMember(dest => dest.Price, opt => opt.Condition((src, dest, srcMember) => src.Price.HasValue))
            .ForMember(dest => dest.BookingDeadline, opt => opt.Condition((src, dest, srcMember) => src.BookingDeadline.HasValue))
            .ForMember(dest => dest.Title, opt => opt.Condition((src, dest, srcMember) => src.Title != null))
            .ForMember(dest => dest.Description, opt => opt.Condition((src, dest, srcMember) => src.Description != null))
            .ForMember(dest => dest.EventType, opt => opt.Condition((src, dest, srcMember) => src.EventType.HasValue))
            .ForMember(dest => dest.Location, opt => opt.Condition((src, dest, srcMember) => src.Location != null))
            .ForMember(dest => dest.Capacity, opt => opt.Condition((src, dest, srcMember) => src.Capacity.HasValue))
            .ForMember(dest => dest.Metadata, opt => opt.Condition((src, dest, srcMember) => src.Metadata != null));
            CreateMap<EventCreateAdminDto, Event>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
