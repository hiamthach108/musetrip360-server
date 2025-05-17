namespace Application.Workers;

using Application.DTOs.Notification;
using Application.Service;
using Application.Shared.Constant;
using Application.Shared.Helpers;
using Core.Queue;

public class NotificationWorker : BackgroundService
{
  private readonly IQueueSubscriber _queueSubscriber;
  private readonly ILogger<NotificationWorker> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public NotificationWorker(
    IQueueSubscriber queueSubscriber,
    ILogger<NotificationWorker> logger,
    IServiceScopeFactory scopeFactory
  )
  {
    _logger = logger;
    _queueSubscriber = queueSubscriber;
    _scopeFactory = scopeFactory;
    _logger.LogInformation("NotificationWorker initialized");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("NotificationWorker started");

    // run task in background and subscribe to the queue
    var result = await _queueSubscriber.Subscribe(QueueConst.Notification, async (message) =>
    {
      // Create a new scope for each message processing to ensure that the DbContext is disposed of properly
      // and to avoid any potential memory leaks
      using var scope = _scopeFactory.CreateScope();
      var messagingService = scope.ServiceProvider.GetRequiredService<IMessagingService>();

      try
      {
        var notification = JsonHelper.DeserializeObject<CreateNotificationDto>(message);
        if (notification == null)
        {
          throw new Exception("Failed to deserialize notification");
        }

        // call to messaging service to handle the notification
        var data = await messagingService.HandleCreateNotification(notification);

        if (data == null)
        {
          throw new Exception("Failed to push notification");
        }

        _logger.LogInformation("Notification sent successfully");
      }
      catch (Exception ex)
      {
        // Handle exception
        _logger.LogError(ex, "Failed to process notification");
      }
    }, stoppingToken);

    if (!result.Success)
    {
      _logger.LogError("Failed to subscribe to notification queue: {ErrorMessage}", result.ErrorMessage);
      throw new Exception($"Failed to subscribe to notification queue: {result.ErrorMessage}");
    }
    _logger.LogInformation($"Subscribed to [{QueueConst.Notification}] queue successfully");
  }
}