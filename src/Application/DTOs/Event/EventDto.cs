using Application.Shared.Enum;

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
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ArtifactDto> Artifacts { get; set; } = null!;
    //   public ICollection<TourOnlineDto> TourOnlines { get; set; } = null!;
    //   public ICollection<TourGuideDto> TourGuides { get; set; } = null!;
    //   public ICollection<TicketAddonDto> TicketAddons { get; set; } = null!;
}
