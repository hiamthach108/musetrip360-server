namespace Infrastructure.Repository;

using Domain.Messaging;
using Database;
using Microsoft.EntityFrameworkCore;
using Application.DTOs.Notification;
using Application.Shared.Enum;

public interface INotificationRepository
{
  NotificationList GetUserNotifications(NotificationQuery query, Guid userId);
  Task<Notification> CreateNotification(Notification notification);
  Task<Notification> UpdateReadStatus(Guid notificationId, bool isRead);
  Task<bool> DeleteNotification(Guid notificationId);
}

public class NotificationList
{
  public IEnumerable<Notification> Notifications { get; set; } = [];
  public int Total { get; set; }
  public int TotalUnread { get; set; }
}

public class NotificationRepository : INotificationRepository
{
  private readonly MuseTrip360DbContext _context;

  public NotificationRepository(MuseTrip360DbContext context)
  {
    _context = context;
  }

  public NotificationList GetUserNotifications(NotificationQuery query, Guid userId)
  {
    int page = query.Page < 1 ? 1 : query.Page;
    int pageSize = query.PageSize <= 0 ? 10 : query.PageSize;

    var q = _context.Notifications
      .Where(n => (n.Target == NotificationTargetEnum.User && n.UserId == userId) || n.Target == NotificationTargetEnum.All)
      .AsQueryable();

    if (query.IsRead.HasValue)
      q = q.Where(n => n.IsRead == query.IsRead.Value);

    var total = q.Count();
    var totalUnread = q.Where(n => !n.IsRead).Count();
    var notifications = q
      .OrderByDescending(n => n.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToList();

    return new NotificationList
    {
      Notifications = notifications,
      Total = total,
      TotalUnread = totalUnread
    };
  }

  public async Task<Notification> CreateNotification(Notification notification)
  {
    await _context.Notifications.AddAsync(notification);
    await _context.SaveChangesAsync();
    return notification;
  }

  public async Task<Notification> UpdateNotification(Notification notification)
  {
    _context.Notifications.Update(notification);
    await _context.SaveChangesAsync();
    return notification;
  }

  public async Task<Notification> UpdateReadStatus(Guid notificationId, bool isRead)
  {
    var notification = await _context.Notifications.FindAsync(notificationId);
    if (notification == null)
    {
      throw new Exception("Notification not found");
    }
    notification.IsRead = isRead;
    await _context.SaveChangesAsync();
    return notification;
  }

  public async Task<bool> DeleteNotification(Guid notificationId)
  {
    var notification = await _context.Notifications.FindAsync(notificationId);
    if (notification == null)
    {
      return false;
    }

    _context.Notifications.Remove(notification);
    await _context.SaveChangesAsync();
    return true;
  }
}
