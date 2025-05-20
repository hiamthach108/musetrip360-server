using System.Text.Json;
using Application.Shared.Enum;
using AutoMapper;
using Domain.Events;

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
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ArtifactDto>? Artifacts { get; set; }
    public ICollection<TourOnlineDto>? TourOnlines { get; set; }
    //   public ICollection<TourGuideDto> TourGuides { get; set; } = null!;
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
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EventCreateAdminDto, Event>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
