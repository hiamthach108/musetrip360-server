namespace Core.HttpClient;

public class ExternalServicesConfig
{
  public Dictionary<string, HttpServiceConfig> Services { get; set; } = new();
}

public class HttpServiceConfig
{
  public string BaseUrl { get; set; } = null!;
  public string? ApiKey { get; set; }
  public string? BearerToken { get; set; }
  public string? Username { get; set; }
  public string? Password { get; set; }
  public int TimeoutMs { get; set; } = 30000;
  public int RetryCount { get; set; } = 3;
  public int RetryDelayMs { get; set; } = 1000;
  public Dictionary<string, string> DefaultHeaders { get; set; } = new();
  public bool LogRequestResponse { get; set; } = true;
}