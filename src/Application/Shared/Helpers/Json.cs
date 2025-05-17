using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Shared.Helpers;

public static class JsonHelper
{
  public static T? DeserializeObject<T>(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
      });
    }
    catch (JsonException ex)
    {
      Console.WriteLine($"Error deserializing JSON: {ex.Message}");
      return default;
    }
  }

  public static string SerializeObject<T>(T obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
      });
    }
    catch (JsonException ex)
    {
      Console.WriteLine($"Error serializing object: {ex.Message}");
      return string.Empty;
    }
  }
  public static string SerializeObject(object obj)
  {
    try
    {
      return JsonSerializer.Serialize(obj, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
      });
    }
    catch (JsonException ex)
    {
      Console.WriteLine($"Error serializing object: {ex.Message}");
      return string.Empty;
    }
  }

  public static T? DeserializeObject<T>(string json, JsonSerializerOptions options)
  {
    try
    {
      return JsonSerializer.Deserialize<T>(json, options);
    }
    catch (JsonException ex)
    {
      Console.WriteLine($"Error deserializing JSON: {ex.Message}");
      return default;
    }
  }

  public static T? DeserializeObject<T>(object obj)
  {
    try
    {
      return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(obj), new JsonSerializerOptions
      {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
      });
    }
    catch (JsonException ex)
    {
      Console.WriteLine($"Error deserializing JSON: {ex.Message}");
      return default;
    }
  }
}