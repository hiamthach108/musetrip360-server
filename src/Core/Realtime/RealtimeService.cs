namespace Core.Realtime;

using System.Text.Json;
using Application.DTOs;
using Application.DTOs.Chat;
using Microsoft.AspNetCore.SignalR;

public interface IRealtimeService
{
  Task SendMessage(MessageDto msg, List<string> userIds);
  Task SendMessageToUser(MessageDto msg, string userId);
  Task MulticastMessage(MessageDto msg, string[] userIds);
}

public class RealtimeService : IRealtimeService
{
  private readonly ILogger<RealtimeService> _logger;
  private readonly IHubContext<ChatHub> _hubCtx;

  public RealtimeService(ILogger<RealtimeService> logger, IHubContext<ChatHub> hubCtx)
  {
    _logger = logger;
    _hubCtx = hubCtx;
  }

  public async Task SendMessage(MessageDto msg, List<string> userIds)
  {
    if (userIds.Count > 1)
    {
      await MulticastMessage(msg, [.. userIds]);
    }
    else
    {
      await SendMessageToUser(msg, userIds[0]);
    }
  }

  public async Task SendMessageToUser(MessageDto msg, string userId)
  {
    _logger.LogInformation($"Sending message to user {userId}");
    _logger.LogInformation(JsonSerializer.Serialize(ChatHub.UserConn));
    try
    {
      if (ChatHub.UserConn.TryGetValue(userId, out var connectionIds))
      {
        foreach (var connectionId in connectionIds)
        {
          await _hubCtx.Clients.Client(connectionId).SendAsync("ReceiveMessage", msg);
        }
      }
    }
    catch (Exception e)
    {
      _logger.LogError(e.Message);
    }
  }

  public async Task MulticastMessage(MessageDto msg, string[] userIds)
  {
    foreach (var userId in userIds)
    {
      await SendMessageToUser(msg, userId);
    }
  }
}