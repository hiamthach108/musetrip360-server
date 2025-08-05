using Application.Shared.Enum;

public class EventParticipantCreateDto
{
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRoleEnum Role { get; set; }
}