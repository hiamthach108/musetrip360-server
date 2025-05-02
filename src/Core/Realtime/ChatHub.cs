namespace Core.Realtime;

using System.Collections.Concurrent;
using Core.Jwt;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
  private readonly ILogger<ChatHub> _logger;
  public static ConcurrentDictionary<string, List<string>> UserConn = [];
  private readonly IJwtService _jwtSvc;

  public ChatHub(ILogger<ChatHub> logger, IJwtService jwtService)
  {
    _logger = logger;
    _jwtSvc = jwtService;
  }

  public async Task Ping()
  {
    await Clients.Caller.SendAsync("Pong");
  }

  public async Task SendMessageToUser(string recipientId, string message)
  {
    if (UserConn.TryGetValue(recipientId, out var connectionIds))
    {
      var (isValid, senderPayload) = ValidateConnection();
      if (!isValid || senderPayload == null)
      {
        Context.Abort();
        return;
      }

      foreach (var connectionId in connectionIds)
      {
        await Clients.Client(connectionId).SendAsync("ReceiveMessage", senderPayload.UserId.ToString(), message);
      }
    }
  }

  public async Task SendMessageToGroup(string groupName, string message)
  {
    var (isValid, senderPayload) = ValidateConnection();
    if (!isValid || senderPayload == null)
    {
      Context.Abort();
      return;
    }

    await Clients.Group(groupName).SendAsync("ReceiveMessage", senderPayload.UserId.ToString(), message);
  }

  public override async Task OnConnectedAsync()
  {
    var (isValid, payload) = ValidateConnection();
    if (!isValid || payload == null)
    {
      _logger.LogError("[ChatHub] Invalid connection");
      Context.Abort();
      return;
    }

    if (UserConn.TryGetValue(payload.UserId.ToString(), out var connectionIds))
    {
      connectionIds.Add(Context.ConnectionId);
    }
    else
    {
      UserConn[payload.UserId.ToString()] = new List<string> { Context.ConnectionId };
    }

    _logger.LogInformation($"[ChatHub] Connection: Id {Context.ConnectionId}; User {payload.UserId}");
    await base.OnConnectedAsync();
  }

  public override async Task OnDisconnectedAsync(Exception? exception)
  {
    var (isValid, payload) = ValidateConnection();
    if (!isValid || payload == null)
    {
      _logger.LogError("[ChatHub] Invalid connection");
      Context.Abort();
      return;
    }

    if (UserConn.TryGetValue(payload.UserId.ToString(), out var connectionIds))
    {
      connectionIds.Remove(Context.ConnectionId);
    }
    _logger.LogInformation($"[ChatHub] Disconnection: Id {Context.ConnectionId}; User {payload.UserId}");
    await base.OnDisconnectedAsync(exception);
  }

  public async Task JoinGroup(string groupName)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    _logger.LogInformation($"[ChatHub] User {Context.ConnectionId} joined group {groupName}");
  }

  public async Task JoinAdminGroup()
  {
    var (isValid, senderPayload) = ValidateConnection();
    if (!isValid || senderPayload == null)
    {
      _logger.LogError("[ChatHub] Invalid connection");
      Context.Abort();
      return;
    }

    await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
  }

  private (bool isValid, Payload? payload) ValidateConnection()
  {
    try
    {
      var httpContext = Context.GetHttpContext();
      if (httpContext == null)
      {
        return (false, null);
      }

      var token = httpContext.Request.Query["access_token"].ToString();
      if (string.IsNullOrEmpty(token))
      {
        return (false, null);
      }

      var payload = _jwtSvc.ValidateToken(token);
      return (payload != null, payload);
    }
    catch (System.Exception)
    {
      return (false, null);
    }

  }
}