using Application.Shared.Enum;

public class EventParticipantUpdateDto
{
    public ParticipantRoleEnum Role { get; set; }
    public ParticipantStatusEnum Status { get; set; }
}