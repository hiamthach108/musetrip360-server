using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

public class SfuConnection
{
    private ClientWebSocket _ws;
    private readonly ILogger _logger;
    // init value when sfu connect to hub
    private string _connectionId = string.Empty;
    private string _roomId = string.Empty;
    private int _nextId = 1;
    private int GetNextId() => Interlocked.Increment(ref _nextId);
    private readonly IHubContext<SignalingHub> _hubContext;
    private bool isInited = true;

    public SfuConnection(ClientWebSocket ws, ILogger logger, IHubContext<SignalingHub> hubContext)
    {
        _ws = ws;
        _logger = logger;
        _hubContext = hubContext;
    }

    public void SetConnectionId(string connectionId) => _connectionId = connectionId;

    public void SetRoomId(string roomId) => _roomId = roomId;

    public string GetRoomId() => _roomId;

    public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Connecting to SFU at {uri}");
            // connect to sfu 
            await _ws.ConnectAsync(uri, cancellationToken);
            // start receive loop
            _ = Task.Run(ReceiveLoopAsync);
            _logger.LogInformation($"Successfully connected to SFU at {uri}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to connect to SFU at {uri}");
            throw;
        }
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];

        while (_ws.State == WebSocketState.Open)
        {
            using var ms = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await _ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                    return;
                }

                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            var json = Encoding.UTF8.GetString(ms.ToArray());
            await HandleIncomingJsonRpc(json);
        }
    }

    private async Task HandleIncomingJsonRpc(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        _logger.LogInformation($"Received JSON-RPC message: {json}");

        if (root.TryGetProperty("method", out var methodProp))
        {
            var method = methodProp.GetString();
            switch (method)
            {
                case "trickle":
                    var trickleParams = root.GetProperty("params");
                    _ = _hubContext.Clients.Client(_connectionId).SendAsync("ReceiveIceCandidate", _connectionId, trickleParams.ToString(), isInited);

                    break;
                case "offer":
                    if (isInited)
                    {
                        isInited = false;
                    }
                    var offerParams = root.GetProperty("params");
                    // Forward to client
                    await _hubContext.Clients.Client(_connectionId).SendAsync("ReceiveOffer", _connectionId, offerParams.ToString());
                    break;
                default:
                    _logger.LogError($"Unhandled method: {method}");
                    break;
            }
        }
        else if (root.TryGetProperty("result", out var resultProp))
        {
            if (resultProp.TryGetProperty("type", out var typeProp))
            {
                var type = typeProp.GetString();
                var sdp = resultProp.GetProperty("sdp").GetString();

                switch (type)
                {
                    case "answer":
                        await _hubContext.Clients.Client(_connectionId).SendAsync("ReceiveAnswer", _connectionId, resultProp.ToString());
                        break;
                    default:
                        _logger.LogError($"Unhandled result type: {type}");
                        break;
                }
            }
            else
            {
                _logger.LogError("Missing 'type' in result.");
            }
        }
        else
        {
            _logger.LogError("Unknown message structure from SFU: " + json);
        }
    }

    public async Task SendJsonRpcAsync(string json)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            _logger.LogDebug("Sent JSON-RPC message: {Json}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send JSON-RPC message: {Json}", json);
            throw;
        }
    }

    public async Task JoinRoomAsync(string roomId, string userId, string offerSdpJson)
    {
        _logger.LogInformation($"Joining room {roomId} with user {userId}");
        // deserialize offer sdp json to sdp object
        var sdpObject = JsonSerializer.Deserialize<SdpParams>(offerSdpJson) ?? throw new Exception("Invalid offer SDP");
        // create join params
        var joinParams = new JoinParams
        {
            Sid = roomId,
            Uid = userId,
            Offer = sdpObject
        };

        // create rpc request
        var joinRequest = new JsonRpcRequest<JoinParams>(
            id: GetNextId(),
            method: "join",
            parameters: joinParams
        );
        // send join request to sfu
        await SendJsonRpcAsync(joinRequest.ToJson());
        _logger.LogInformation($"Join room request sent for room {roomId}");
    }

    public async Task Offer(string sdp)
    {
        _logger.LogInformation("Sending offer");
        // create offer params
        var offerParams = new
        {
            desc = new
            {
                type = "offer",
                sdp
            }
        };
        // create rpc request
        var rpc = new JsonRpcRequest<object>(
            id: GetNextId(),
            method: "offer",
            parameters: offerParams
        );
        // send offer request to sfu
        await SendJsonRpcAsync(rpc.ToJson());
        _logger.LogInformation("Offer sent successfully");
    }

    public async Task Answer(string sdp)
    {
        _logger.LogInformation("Sending answer");
        // create answer params
        var answerParams = new
        {
            desc = new
            {
                type = "answer",
                sdp
            }
        };
        // create rpc request
        var rpc = new JsonRpcRequest<object>(
            id: GetNextId(),
            method: "answer",
            parameters: answerParams
        );

        // send answer request to sfu
        await SendJsonRpcAsync(rpc.ToJson());
        _logger.LogInformation("Answer sent successfully");
    }

    public void Trickle(string candidateJson, int target)
    {
        _logger.LogDebug($"Sending trickle candidate: {candidateJson}");
        // parse candidate string to ice candidate object
        var candidate = JsonSerializer.Deserialize<IceCandidate>(candidateJson);
        // create trickle params
        var trickleParams = new
        {
            candidate,
            target
        };
        // create rpc request
        var rpc = new JsonRpcRequest<object>(
            id: GetNextId(),
            method: "trickle",
            parameters: trickleParams
        );
        // send trickle request to sfu
        _ = SendJsonRpcAsync(rpc.ToJson());
        _logger.LogDebug($"Trickle candidate sent");
    }

    internal async Task CloseAsync()
    {
        // dispose ws
        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
    }
}