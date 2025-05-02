namespace Core.Json;
using System;
using Newtonsoft.Json;

public interface IJsonService
{
  string Serialize<T>(T obj);
  T Deserialize<T>(string json);
}

public class JsonService : IJsonService
{
  public string Serialize<T>(T obj)
  {
    if (obj == null)
      throw new ArgumentNullException(nameof(obj), "Object to serialize cannot be null.");

    try
    {
      return JsonConvert.SerializeObject(obj);
    }
    catch (Exception ex)
    {
      // Handle or log the exception as needed
      throw new InvalidOperationException("Serialization failed.", ex);
    }
  }

  public T Deserialize<T>(string json)
  {
    return JsonConvert.DeserializeObject<T>(json) ?? throw new InvalidOperationException("Deserialization failed.");
  }
}