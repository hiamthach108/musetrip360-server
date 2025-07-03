namespace Core.HttpClient;

using System.Text;
using System.Text.Json;

public interface IHttpClientService
{
  // Generic typed methods
  Task<T?> GetAsync<T>(string uri, Dictionary<string, string>? headers = null);
  Task<T?> PostAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null);
  Task<T?> PutAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null);
  Task<T?> DeleteAsync<T>(string uri, Dictionary<string, string>? headers = null);
  Task<T?> PatchAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null);
}

public class HttpClientService : IHttpClientService
{
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger<HttpClientService> _logger;
  private readonly string _baseUrl;
  private readonly JsonSerializerOptions _jsonOptions;

  public HttpClientService(
    string baseUrl,
    IHttpClientFactory httpClientFactory,
    ILogger<HttpClientService> logger)
  {
    _baseUrl = baseUrl;
    _httpClientFactory = httpClientFactory;
    _logger = logger;

    _jsonOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    };
  }

  public async Task<T?> GetAsync<T>(string uri, Dictionary<string, string>? headers = null)
  {
    var content = await GetAsync(uri, headers);
    return DeserializeResponse<T>(content);
  }

  public async Task<T?> PostAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    var content = await PostAsync(uri, body, headers);
    return DeserializeResponse<T>(content);
  }

  public async Task<T?> PutAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    var content = await PutAsync(uri, body, headers);
    return DeserializeResponse<T>(content);
  }

  public async Task<T?> DeleteAsync<T>(string uri, Dictionary<string, string>? headers = null)
  {
    var content = await DeleteAsync(uri, headers);
    return DeserializeResponse<T>(content);
  }

  public async Task<T?> PatchAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    var content = await PatchAsync(uri, body, headers);
    return DeserializeResponse<T>(content);
  }

  public async Task<string?> GetAsync(string uri, Dictionary<string, string>? headers = null)
  {
    return await ExecuteRequestAsync(HttpMethod.Get, uri, null, headers);
  }

  public async Task<string?> PostAsync(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    return await ExecuteRequestAsync(HttpMethod.Post, uri, body, headers);
  }

  public async Task<string?> PutAsync(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    return await ExecuteRequestAsync(HttpMethod.Put, uri, body, headers);
  }

  public async Task<string?> DeleteAsync(string uri, Dictionary<string, string>? headers = null)
  {
    return await ExecuteRequestAsync(HttpMethod.Delete, uri, null, headers);
  }

  public async Task<string?> PatchAsync(string uri, object? body = null, Dictionary<string, string>? headers = null)
  {
    return await ExecuteRequestAsync(HttpMethod.Patch, uri, body, headers);
  }

  private async Task<string?> ExecuteRequestAsync(
    HttpMethod method,
    string uri,
    object? body = null,
    Dictionary<string, string>? headers = null)
  {
    try
    {
      var httpClient = _httpClientFactory.CreateClient();
      var fullUrl = $"{_baseUrl.TrimEnd('/')}/{uri.TrimStart('/')}";

      var request = new HttpRequestMessage(method, fullUrl);

      // Add headers
      if (headers != null)
      {
        foreach (var header in headers)
        {
          request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
      }

      // Add body for POST, PUT, PATCH
      if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
      {
        var jsonContent = JsonSerializer.Serialize(body, _jsonOptions);
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
      }

      _logger.LogInformation("HTTP {Method} request to {Url}", method, fullUrl);

      var response = await httpClient.SendAsync(request);
      var content = await response.Content.ReadAsStringAsync();

      if (response.IsSuccessStatusCode)
      {
        _logger.LogInformation("HTTP {Method} success: {StatusCode}", method, response.StatusCode);
        return content;
      }

      _logger.LogWarning("HTTP {Method} failed: {StatusCode} - {Content}", method, response.StatusCode, content);
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "HTTP {Method} exception for {BaseUrl}/{Uri}", method, _baseUrl, uri);
      return null;
    }
  }

  private T? DeserializeResponse<T>(string? content)
  {
    if (string.IsNullOrEmpty(content))
      return default;

    try
    {
      return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }
    catch (JsonException ex)
    {
      _logger.LogError(ex, "Failed to deserialize response to type {Type}", typeof(T).Name);
      return default;
    }
  }
}