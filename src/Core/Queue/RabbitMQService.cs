namespace Core.Queue;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class RabbitMqPublisher : IQueuePublisher, IDisposable
{
  private readonly IChannel _channel;
  private readonly RabbitMQConnection _connection;
  private readonly ILogger<RabbitMqPublisher> _logger;

  public RabbitMqPublisher(RabbitMQConnection connection, ILogger<RabbitMqPublisher> logger)
  {
    _connection = connection;
    _channel = _connection.Connection.CreateChannelAsync().Result;
    _logger = logger;
  }

  public async Task<QueueOperationResult> Publish(string queueName, object message, CancellationToken cancellationToken = default)
  {
    try
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return QueueOperationResult.Fail("Operation cancelled");
      }

      await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
      var json = JsonSerializer.Serialize(message);
      var body = Encoding.UTF8.GetBytes(json);
      // _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
      await _channel.BasicPublishAsync("", queueName, body, cancellationToken);
      _logger.LogInformation($"[x] Sent message to {queueName}: {json}");
      return QueueOperationResult.Ok();
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error publishing to {queueName}: {ex.Message}");
      return QueueOperationResult.Fail(ex.Message);
    }
  }

  public void Dispose()
  {
    _channel?.CloseAsync();
    _channel?.Dispose();
  }
}

public class RabbitMqSubscriber : IQueueSubscriber, IDisposable
{
  private readonly IChannel _channel;
  private readonly RabbitMQConnection _connection;
  private readonly ILogger<RabbitMqSubscriber> _logger;

  public RabbitMqSubscriber(RabbitMQConnection connection, ILogger<RabbitMqSubscriber> logger)
  {
    _connection = connection;
    _channel = _connection.Connection.CreateChannelAsync().Result;
    _logger = logger;
  }

  public async Task<QueueOperationResult> Subscribe(string queueName, Action<object> onMessage, CancellationToken cancellationToken = default)
  {
    try
    {
      // _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null); 
      await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
      await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);
      var consumer = new AsyncEventingBasicConsumer(_channel);
      consumer.ReceivedAsync += async (model, ea) =>
      {
        if (cancellationToken.IsCancellationRequested)
        {
          await _channel.BasicCancelAsync(consumer.ConsumerTags[0]);
          return;
        }
        try
        {
          var body = ea.Body.ToArray();
          var json = Encoding.UTF8.GetString(body);
          var message = JsonSerializer.Deserialize<object>(json); // Returns JsonElement or primitive
          if (message != null)
          {
            onMessage(message);
            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
            _logger.LogInformation($"[x] Received from {queueName}: {json}");
          }
          else
          {
            await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
            _logger.LogError($"[x] Failed to deserialize message from {queueName}");
          }
        }
        catch (Exception ex)
        {
          _logger.LogError($"Error processing message from {queueName}: {ex.Message}");
          await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
        }
      };
      await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
      _logger.LogInformation($"[*] Subscribed to {queueName}");
      return QueueOperationResult.Ok();
    }
    catch (Exception ex)
    {
      _logger.LogError($"Error subscribing to {queueName}: {ex.Message}");
      return QueueOperationResult.Fail(ex.Message);
    }
  }

  public void Dispose()
  {
    _channel?.CloseAsync();
    _channel?.Dispose();
  }
}
