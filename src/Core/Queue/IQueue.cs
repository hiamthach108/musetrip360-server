namespace Core.Queue;

public class QueueOperationResult
{
  public bool Success { get; init; }
  public string ErrorMessage { get; init; } = string.Empty;

  public static QueueOperationResult Ok() => new() { Success = true };
  public static QueueOperationResult Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}

public interface IQueuePublisher
{
  Task<QueueOperationResult> Publish(string queueName, object message, CancellationToken cancellationToken = default);
}

public interface IQueueSubscriber
{
  Task<QueueOperationResult> Subscribe(string queueName, Action<object> onMessage, CancellationToken cancellationToken = default);
}