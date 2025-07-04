# Custom HTTP Client Service Implementation Plan

## Overview
Create a comprehensive HTTP client service in `src/Core/HttpClient/` for calling external services with proper configuration, error handling, and logging.

## Implementation Steps

### 1. HTTP Client Service Creation ✅ COMPLETED
Created in `src/Core/HttpClient/`:
- `IHttpClientService`: Interface defining HTTP operations ✅
- `HttpClientService`: Implementation with HttpClient wrapper ✅
- `HttpClientConfig`: Configuration model for different external services ✅

### 2. Features to Implement ✅ COMPLETED
- **GET, POST, PUT, DELETE operations**: Full REST API support ✅
- **Generic response handling**: Support for typed responses ✅
- **Authentication support**: Bearer token, API key, basic auth ✅
- **Request/Response logging**: Structured logging for debugging ✅
- **Error handling**: Proper exception handling and retry logic ✅
- **Timeout configuration**: Configurable timeouts per service ✅
- **Base URL management**: Support for multiple external services ✅
- **Header management**: Custom headers per request or service ✅
- **Serialization**: JSON serialization/deserialization ✅

### 3. Configuration Structure
Support multiple external service configurations:
```json
{
  "ExternalServices": {
    "PaymentGateway": {
      "BaseUrl": "https://api.payment.com",
      "ApiKey": "xxx",
      "Timeout": 30000,
      "RetryCount": 3
    },
    "NotificationService": {
      "BaseUrl": "https://notifications.api.com",
      "BearerToken": "xxx",
      "Timeout": 15000
    }
  }
}
```

### 4. Error Handling Strategy
- Custom exception types for different HTTP error scenarios
- Retry logic for transient failures
- Circuit breaker pattern for external service failures
- Proper logging of requests/responses

### 5. Usage Pattern
Services should be able to inject and use like:
```csharp
public class SomeService : BaseService
{
    private readonly IHttpClientService _httpClient;
    
    public async Task<ExternalApiResponse> CallExternalApi()
    {
        return await _httpClient.GetAsync<ExternalApiResponse>("PaymentGateway", "/api/endpoint");
    }
}
```

## Technical Requirements
- Follow existing Core layer patterns
- Use IConfiguration for settings
- Implement proper DI registration
- Include comprehensive logging
- Support async operations
- Provide both generic and non-generic methods

## Files Created ✅ COMPLETED
1. `src/Core/HttpClient/IHttpClientService.cs` - Interface ✅
2. `src/Core/HttpClient/HttpClientService.cs` - Implementation ✅  
3. `src/Core/HttpClient/Models/HttpClientConfig.cs` - Configuration models ✅
4. `src/Core/HttpClient/Models/HttpRequest.cs` - Request models ✅
5. `src/Core/HttpClient/Models/HttpResponse.cs` - Response models ✅
6. `src/Core/HttpClient/Exceptions/HttpClientException.cs` - Custom exceptions ✅

## Security Features Implemented ✅
- Secure storage of API keys and tokens ✅
- Request/response filtering for sensitive data in logs ✅
- SSL/TLS verification (via HttpClient) ✅
- Multiple authentication methods support ✅

## Implementation Summary

✅ **COMPLETED**: Custom HTTP client service successfully implemented in Core layer

### Features Implemented:
- **Multiple HTTP Methods**: GET, POST, PUT, DELETE, PATCH with both generic and non-generic versions
- **Configuration-Based**: External service configs loaded from appsettings.json
- **Multiple Authentication**: Bearer token, API key, Basic auth support
- **Retry Logic**: Configurable retry attempts with exponential backoff
- **Timeout Management**: Per-service and per-request timeout configuration
- **Comprehensive Logging**: Request/response logging with configurable levels
- **Error Handling**: Custom exceptions and proper error responses
- **Health Checks**: Built-in service health monitoring
- **Header Management**: Default headers per service + custom headers per request

### Usage Example:
```csharp
// In any service class
public class ExampleService : BaseService
{
    private readonly IHttpClientService _httpClient;
    
    public ExampleService(IHttpClientService httpClient, ...)
    {
        _httpClient = httpClient;
    }
    
    public async Task<ExternalApiResponse> CallExternalApiAsync()
    {
        // Typed response
        var result = await _httpClient.GetAsync<ExternalApiResponse>("PaymentGateway", "/api/payments");
        
        if (result.IsSuccess)
        {
            return result.Data;
        }
        
        throw new Exception(result.ErrorMessage);
    }
    
    public async Task<string> PostDataAsync(object data)
    {
        // Non-typed response
        var result = await _httpClient.PostAsync("NotificationService", "/api/notify", data);
        return result.RawContent;
    }
}
```

### Configuration Example (appsettings.json):
```json
{
  "ExternalServices": {
    "PaymentGateway": {
      "BaseUrl": "https://api.payments.com",
      "ApiKey": "your-api-key",
      "TimeoutMs": 30000,
      "RetryCount": 3,
      "RetryDelayMs": 1000,
      "LogRequestResponse": true
    },
    "NotificationService": {
      "BaseUrl": "https://notifications.api.com",
      "BearerToken": "your-bearer-token",
      "TimeoutMs": 15000,
      "DefaultHeaders": {
        "X-Custom-Header": "value"
      }
    }
  }
}
```

### ✅ SIMPLIFIED VERSION - Main HttpClientService Updated

**BREAKING CHANGE**: Simplified the main `HttpClientService` to focus on essentials:
- **Base URL in Constructor**: Pass base URL when creating the service instance
- **Simple Interface**: Just URI, headers, and generic responses
- **No Complex Configuration**: Removed service configs, retry logic, etc.
- **Clean API**: Direct and straightforward to use

### Updated Interface:
```csharp
public interface IHttpClientService
{
    // Generic typed methods
    Task<T?> GetAsync<T>(string uri, Dictionary<string, string>? headers = null);
    Task<T?> PostAsync<T>(string uri, object? body = null, Dictionary<string, string>? headers = null);
    // ... other HTTP methods
    
    // Non-generic methods returning raw string
    Task<string?> GetAsync(string uri, Dictionary<string, string>? headers = null);
    Task<string?> PostAsync(string uri, object? body = null, Dictionary<string, string>? headers = null);
    // ... other HTTP methods
}
```

### Constructor with Base URL:
```csharp
public HttpClientService(
    string baseUrl,
    IHttpClientFactory httpClientFactory,
    ILogger<HttpClientService> logger)
```

### Usage Example:
```csharp
public class ExampleService : BaseService
{
    private readonly IHttpClientService _httpClient;
    
    public ExampleService(IHttpClientFactory httpClientFactory, ILogger<HttpClientService> logger)
    {
        // Create client with base URL
        _httpClient = new HttpClientService("https://api.payments.com", httpClientFactory, logger);
    }
    
    public async Task<PaymentResponse> GetPaymentAsync(string paymentId)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", "Bearer your-token" },
            { "X-API-Key", "your-api-key" }
        };
        
        // Just pass the URI (base URL already set in constructor)
        var result = await _httpClient.GetAsync<PaymentResponse>($"/api/payments/{paymentId}", headers);
        return result;
    }
    
    public async Task<string> PostDataAsync(object data)
    {
        // Raw response
        var result = await _httpClient.PostAsync("/api/notify", data);
        return result;
    }
}
```

### DI Registration:
```csharp
builder.Services.AddHttpClient();
// Note: You'll need to register specific instances or use a factory pattern
// since the base URL is passed in constructor
``` 