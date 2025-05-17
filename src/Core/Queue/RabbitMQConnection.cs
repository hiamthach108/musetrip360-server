namespace Core.Queue;

using RabbitMQ.Client;
using System;

public class RabbitMQConnection : IDisposable
{
  public IConnection Connection { get; }

  public RabbitMQConnection(IConfiguration configuration)
  {
    var factory = new ConnectionFactory
    {
      HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
      UserName = configuration["RabbitMQ:UserName"] ?? "user",
      Password = configuration["RabbitMQ:Password"] ?? "password",
      Port = configuration.GetValue<int>("RabbitMQ:Port", 5672),
      VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
    };
    Connection = factory.CreateConnectionAsync().Result;
  }

  public void Dispose()
  {
    Connection?.CloseAsync();
    Connection?.Dispose();
  }
}
