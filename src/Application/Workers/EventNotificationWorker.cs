namespace Application.Workers;

using System.Text.Json;
using Application.DTOs.Notification;
using Application.Service;
using Application.Shared.Constant;
using Application.Shared.Enum;
using Core.Queue;
using Database;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

public class EventNotificationWorker : BackgroundService
{
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<EventNotificationWorker> _logger;
  private readonly Timer _timer;

  public EventNotificationWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<EventNotificationWorker> logger
  )
  {
    _scopeFactory = scopeFactory;
    _logger = logger;
    _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
    _logger.LogInformation("EventNotificationWorker initialized");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("EventNotificationWorker started");

    while (!stoppingToken.IsCancellationRequested)
    {
      await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
  }

  private async void DoWork(object? state)
  {
    try
    {
      _logger.LogInformation("EventNotificationWorker is checking for upcoming events...");
      using var scope = _scopeFactory.CreateScope();
      var dbContext = scope.ServiceProvider.GetRequiredService<MuseTrip360DbContext>();
      var queuePublisher = scope.ServiceProvider.GetRequiredService<IQueuePublisher>();

      var now = DateTime.UtcNow;
      var targetTime = now.AddMinutes(30);
      var timeWindow = TimeSpan.FromMinutes(1);

      var eventsStartingSoon = await dbContext.Events
        .Where(e => e.StartTime <= targetTime &&
                   e.StartTime >= targetTime.Subtract(timeWindow) &&
                   e.Status == EventStatusEnum.Published)
        .Select(e => new Event
        {
          Id = e.Id,
          Title = e.Title,
          Location = e.Location,
          StartTime = e.StartTime,
          EndTime = e.EndTime,
          Status = e.Status,
          EventParticipants = e.EventParticipants.Select(ep => new EventParticipant
          {
            Id = ep.Id,
            UserId = ep.UserId,
            EventId = ep.EventId,
            Status = ep.Status
          }).ToList()
        })
        .ToListAsync();

      _logger.LogInformation($"Found {eventsStartingSoon.Count} events starting in 30 minutes");

      foreach (var eventItem in eventsStartingSoon)
      {
        var activeParticipants = eventItem.EventParticipants.ToList();

        _logger.LogInformation($"Event '{eventItem.Title}' has {activeParticipants.Count} confirmed participants");

        foreach (var participant in activeParticipants)
        {
          var notification = new CreateNotificationDto
          {
            Title = "Event Starting Soon",
            Message = $"Your event '{eventItem.Title}' will start in 30 minutes at {eventItem.Location}",
            UserId = participant.UserId,
            Target = NotificationTargetEnum.User,
            Type = "Event",
            Metadata = JsonDocument.Parse(JsonSerializer.Serialize(new
            {
              TargetId = eventItem.Id,
              Event = eventItem
            }))
          };

          var result = await queuePublisher.Publish(QueueConst.Notification, notification);

          if (result.Success)
          {
            _logger.LogInformation($"Notification queued for user {participant.UserId} for event '{eventItem.Title}'");
          }
          else
          {
            _logger.LogError($"Failed to queue notification for user {participant.UserId}: {result.ErrorMessage}");
          }
        }
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred in EventNotificationWorker");
    }
  }

  public override void Dispose()
  {
    _timer?.Dispose();
    base.Dispose();
  }
}