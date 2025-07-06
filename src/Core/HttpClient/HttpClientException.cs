namespace Core.HttpClient;

public class HttpClientException : Exception
{
  public int StatusCode { get; }
  public string? ResponseContent { get; }

  public HttpClientException(string message) : base(message)
  {
    StatusCode = 0;
  }

  public HttpClientException(string message, Exception innerException) : base(message, innerException)
  {
    StatusCode = 0;
  }

  public HttpClientException(int statusCode, string message, string? responseContent = null) : base(message)
  {
    StatusCode = statusCode;
    ResponseContent = responseContent;
  }
}

public class HttpClientConfigurationException : Exception
{
  public HttpClientConfigurationException(string message) : base(message) { }
  public HttpClientConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}

public class HttpClientTimeoutException : HttpClientException
{
  public HttpClientTimeoutException(string message) : base(message) { }
  public HttpClientTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}