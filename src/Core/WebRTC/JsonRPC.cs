using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonRpcRequest<TParams>
{
    public int Id { get; set; }
    public string Method { get; set; }

    [JsonPropertyName("params")]
    public TParams Params { get; set; }

    public JsonRpcRequest(int id, string method, TParams parameters)
    {
        Id = id;
        Method = method;
        Params = parameters;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
public class JoinParams
{
    public string Sid { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;
    public SdpParams Offer { get; set; } = new();
}

public class SdpParams
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("sdp")]
    public string Sdp { get; set; } = string.Empty;
}

public class IceCandidate
{
    [JsonPropertyName("candidate")]
    public string Candidate { get; set; } = string.Empty;

    [JsonPropertyName("sdpMid")]
    public string SdpMid { get; set; } = string.Empty;

    [JsonPropertyName("sdpMLineIndex")]
    public int SdpMLineIndex { get; set; }
}





