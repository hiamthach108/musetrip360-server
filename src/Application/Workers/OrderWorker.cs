namespace Application.Workers;

using Application.DTOs.Payment;
using Application.Service;
using Application.Shared.Constant;
using Application.Shared.Helpers;
using Core.Queue;

public class OrderWorker : BackgroundService
{
  private readonly IQueueSubscriber _queueSubscriber;
  private readonly ILogger<OrderWorker> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public OrderWorker(
    IQueueSubscriber queueSubscriber,
    ILogger<OrderWorker> logger,
    IServiceScopeFactory scopeFactory
  )
  {
    _queueSubscriber = queueSubscriber;
    _logger = logger;
    _scopeFactory = scopeFactory;
    _logger.LogInformation("OrderWorker initialized");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("OrderWorker started");

    // run task in background and subscribe to the queue
    var result = await _queueSubscriber.Subscribe(QueueConst.Order, async (message) =>
    {
      // Create a new scope for each message processing to ensure that the DbContext is disposed of properly
      // and to avoid any potential memory leaks
      using var scope = _scopeFactory.CreateScope();
      var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

      try
      {
        var orderMsg = JsonHelper.DeserializeObject<CreateOrderMsg>(message)
          ?? throw new Exception("Failed to deserialize order message");
        var order = await paymentService.CreateOrder(orderMsg)
          ?? throw new Exception("Failed to create order");

        _logger.LogInformation("Order created successfully: {OrderId}", order.Id);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error processing order message");
      }
    }, stoppingToken);

    if (!result.Success)
    {
      _logger.LogError("Failed to subscribe to order queue: {ErrorMessage}", result.ErrorMessage);
      throw new Exception($"Failed to subscribe to order queue: {result.ErrorMessage}");
    }
    _logger.LogInformation($"Subscribed to [{QueueConst.Order}] queue successfully");
  }
}