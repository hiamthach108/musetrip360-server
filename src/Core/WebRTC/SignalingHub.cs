using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Core.Jwt;
using Microsoft.AspNetCore.SignalR;

public class SignalingHub : Hub
{
    private readonly ILogger<SignalingHub> _logger;
    private static ConcurrentDictionary<string, SfuConnection> _connections = new();
    private static ConcurrentDictionary<string, string> _peerIdToStreamId = new();
    private static ConcurrentDictionary<string, string> _streamIdToPeerId = new();
    private readonly IHubContext<SignalingHub> _hubContext;
    private readonly IJwtService _jwtSvc;
    public SignalingHub(ILogger<SignalingHub> logger, IHubContext<SignalingHub> hubContext, IJwtService jwtSvc)
    {
        _logger = logger;
        _hubContext = hubContext;
        _jwtSvc = jwtSvc;
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
            await sfu.ConnectAsync(new Uri("ws://localhost:7000/ws"));
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

    public void SetStreamPeerId(string streamId)
    {
        _streamIdToPeerId.TryAdd(streamId, Context.ConnectionId);
        _peerIdToStreamId.TryAdd(Context.ConnectionId, streamId);
    }

    public string GetStreamIdByPeerId(string peerId)
    {
        return _peerIdToStreamId[peerId];
    }

    public string GetPeerIdByStreamId(string streamId)
    {
        return _streamIdToPeerId[streamId];
    }

    public void RemoveStreamPeerId()
    {
        _streamIdToPeerId.TryRemove(_peerIdToStreamId[Context.ConnectionId], out _);
        _peerIdToStreamId.TryRemove(Context.ConnectionId, out _);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (_connections.TryRemove(Context.ConnectionId, out var sfu))
            {
                var payload = Context.Items["payload"] as Payload ?? new Payload();
                await Clients.OthersInGroup(sfu.GetRoomId()).SendAsync("PeerDisconnected", payload.UserId, Context.ConnectionId);
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
                // send offer to sfu
                await sfu.JoinRoomAsync(roomId, Context.ConnectionId, offer);
                // set room id for sfu connection
                sfu.SetRoomId(roomId);
                // add peer to room
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                // notify other peers in room that new peer joined
                await Clients.OthersInGroup(roomId).SendAsync("PeerJoined", Context.ConnectionId);
                _logger.LogInformation("Peer joined room {RoomId} for {ConnectionId}", roomId, Context.ConnectionId);
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
