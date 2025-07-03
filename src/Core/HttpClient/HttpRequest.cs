namespace Core.HttpClient;

public class HttpRequestOptions
{
  public Dictionary<string, string> Headers { get; set; } = new();
  public int? TimeoutMs { get; set; }
  public bool? LogRequestResponse { get; set; }
  public string? BearerToken { get; set; }
  public string? ApiKey { get; set; }
}

public class HttpRequestData
{
  public string ServiceName { get; set; } = null!;
  public string Endpoint { get; set; } = null!;
  public object? Body { get; set; }
  public HttpRequestOptions? Options { get; set; }
}

public class HttpResponseResult<T>
{
  public bool IsSuccess { get; set; }
  public T? Data { get; set; }
  public string? ErrorMessage { get; set; }
  public int StatusCode { get; set; }
  public Dictionary<string, string> Headers { get; set; } = new();
  public string? RawContent { get; set; }
}

public class HttpResponseResult : HttpResponseResult<object>
{
}