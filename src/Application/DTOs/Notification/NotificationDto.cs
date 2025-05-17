namespace Application.DTOs.Notification;

using Application.DTOs.Pagination;
using Application.Shared.Enum;
using AutoMapper;
using Domain.Messaging;
public class NotificationDto
{
  public string Title { get; set; } = null!;
  public string Message { get; set; } = null!;
  public string Type { get; set; } = null!;
  public bool IsRead { get; set; }
  public DateTime? ReadAt { get; set; }
  public NotificationTargetEnum Target { get; set; }
  public DateTime CreatedAt { get; set; }
  public Guid UserId { get; set; }
}

public class NotificationQuery : PaginationReq
{
  public bool? IsRead { get; set; }
}

public class NotificationUpdateReadStatusReq
{
  public Guid NotificationId { get; set; }
  public bool IsRead { get; set; }
}

public class NotificationProfile : Profile
{
  public NotificationProfile()
  {
    CreateMap<Notification, NotificationDto>();
    CreateMap<NotificationQuery, Notification>();
  }
}