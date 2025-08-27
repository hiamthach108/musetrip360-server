using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Application.Service;
using Core.Jwt;
using Microsoft.AspNetCore.SignalR;

public class SignalingHub : Hub
{
    private readonly ILogger<SignalingHub> _logger;
    private static ConcurrentDictionary<string, SfuConnection> _connections = new();
    private static ConcurrentDictionary<string, string> _peerIdToStreamId = new();
    private static ConcurrentDictionary<string, string> _streamIdToPeerId = new();
    private static ConcurrentDictionary<string, string> _streamIdToUserId = new();
    private readonly IRoomService _roomService;
    private readonly IHubContext<SignalingHub> _hubContext;
    private readonly IJwtService _jwtSvc;
    private readonly IRoomStateManager _roomStateManager;
    private readonly IUserService _userService;
    private readonly string _sfuUrl;
    public SignalingHub(IConfiguration config, IRoomStateManager roomStateManager, IRoomService roomService, ILogger<SignalingHub> logger, IHubContext<SignalingHub> hubContext, IJwtService jwtSvc, IUserService userService)
    {
        _roomService = roomService;
        _logger = logger;
        _hubContext = hubContext;
        _jwtSvc = jwtSvc;
        _roomStateManager = roomStateManager;
        _userService = userService;
        _sfuUrl = config["SFU:WebSocketUrl"] ?? "";
    }

    // when client connect to hub, init sfu connection and add to dictionary
    // when client disconnect from hub, dispose sfu connection and remove from dictionary and dispose it
    public override async Task OnConnectedAsync()
    {
        try
        {
            // only need to validate on connected
            var (isValid, payload) = ValidateConnection();
            if (!isValid || payload == null)
            {
                _logger.LogError("Invalid connection");
                Context.Abort();
                return;
            }
            else
            {
                Context.Items.Add("payload", payload);
            }
            var socket = new ClientWebSocket();
            // inti sfu connection with hubContext and connectionId
            var sfu = new SfuConnection(socket, _logger, _hubContext);
            // set connection id for sfu connection
            sfu.SetConnectionId(Context.ConnectionId);
            await sfu.ConnectAsync(new Uri(_sfuUrl));
            // add sfu connection to dictionary
            _connections.TryAdd(Context.ConnectionId, sfu);
            _logger.LogInformation($"SFU connected for {Context.ConnectionId}");
            // send connection id to caller
            await Clients.Caller.SendAsync($"ReceiveConnectionId", Context.ConnectionId);
            await Clients.OthersInGroup(sfu.GetRoomId()).SendAsync("PeerJoined", payload.UserId, Context.ConnectionId);
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync");
        }
    }

    public async Task UpdateRoomState(string metadata)
    {
        _connections.TryGetValue(Context.ConnectionId, out var sfu);
        if (sfu == null)
        {
            _logger.LogError("SFU connection not found");
            return;
        }
        //send metadata to room 
        await Clients.OthersInGroup(sfu.GetRoomId()).SendAsync("ReceiveRoomState", metadata);
        var dto = new RoomUpdateMetadataDto
        {
            Metadata = JsonDocument.Parse(metadata)
        };
        await _roomStateManager.UpdateRoomState(sfu.GetRoomId(), dto);
    }

    public void SetStreamPeerId(string streamId)
    {
        var payload = Context.Items["payload"] as Payload ?? new Payload();
        _streamIdToPeerId.TryAdd(streamId, Context.ConnectionId);
        _peerIdToStreamId.TryAdd(Context.ConnectionId, streamId);
        _streamIdToUserId.TryAdd(streamId, payload.UserId.ToString());
    }

    public string GetStreamIdByPeerId(string peerId)
    {
        return _peerIdToStreamId[peerId];
    }

    public string GetPeerIdByStreamId(string streamId)
    {
        return _streamIdToPeerId[streamId];
    }

    public string GetUserByStreamId(string streamId)
    {
        return _streamIdToUserId[streamId];
    }

    public void RemoveStreamPeerId()
    {
        _streamIdToUserId.TryRemove(GetStreamIdByPeerId(Context.ConnectionId), out _);
        _streamIdToPeerId.TryRemove(_peerIdToStreamId[Context.ConnectionId], out _);
        _peerIdToStreamId.TryRemove(Context.ConnectionId, out _);
    }

    public void SendChatMessageToRoom(string roomId, string message)
    {
        Clients.OthersInGroup(roomId).SendAsync("ReceiveChatMessage", message);
    }

    public async Task SendTourActionToRoom(string roomId, string action)
    {
        _ = Clients.OthersInGroup(roomId).SendAsync("ReceiveTourAction", roomId, action);
        await _roomStateManager.HandleUpdateTourState(roomId, action);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (_connections.TryRemove(Context.ConnectionId, out var sfu))
            {
                var payload = Context.Items["payload"] as Payload ?? new Payload();
                await Clients.OthersInGroup(sfu.GetRoomId()).SendAsync("PeerDisconnected", payload.UserId, Context.ConnectionId, _peerIdToStreamId[Context.ConnectionId]);
                // await for client to disconnect from sfu
                await Task.Delay(1000);
                RemoveStreamPeerId();
                // and dispose ws connection
                await sfu.CloseAsync();
                _logger.LogInformation($"SFU connection disposed for {Context.ConnectionId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
        }
    }

    public async Task Join(string roomId, string offer)
    {
        try
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var sfu))
            {
                // validate user before join room
                var payload = Context.Items["payload"] as Payload ?? new Payload();
                var isValid = await _roomService.ValidateUser(payload.UserId, roomId);

                if (!isValid)
                {
                    await Clients.Caller.SendAsync("Error", "User not authorized to join room");
                    return;
                }
                // send offer to sfu
                await sfu.JoinRoomAsync(roomId, Context.ConnectionId, offer);
                // set room id for sfu connection
                sfu.SetRoomId(roomId);
                // add peer to room
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                // notify other peers in room that new peer joined
                await Clients.OthersInGroup(roomId).SendAsync("PeerJoined", payload.UserId, Context.ConnectionId, payload.UserId);
                _logger.LogInformation("Peer joined room {RoomId} for {ConnectionId}", roomId, Context.ConnectionId);
                // get room state and send to peer
                var roomState = await _roomStateManager.GetRoomState(roomId);
                await Clients.Caller.SendAsync("ReceiveRoomState", JsonSerializer.Serialize(roomState));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Join");
        }
    }

    public async Task Answer(string answer)
    {
        try
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var sfu))
            {
                await sfu.Answer(answer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Answer");
        }
    }

    public async Task Offer(string offer)
    {
        try
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var sfu))
            {
                await sfu.Offer(offer);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Offer");
        }
    }

    public void Trickle(string candidate)
    {
        try
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var sfu))
            {
                sfu.Trickle(candidate, 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Trickle");
        }
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