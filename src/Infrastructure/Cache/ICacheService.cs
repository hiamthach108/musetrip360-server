namespace Infrastructure.Cache;

public interface ICacheService
{
  Task<T?> Get<T>(string key);
  Task Set<T>(string key, T value);
  Task Set<T>(string key, T value, TimeSpan expiration);
  Task<bool> Update<T>(string key, T value);
  Task Remove(string key);
  Task<bool> Exists(string key);
  Task Clear();
  Task ClearWithPattern(string pattern);
  Task ForceLogout(Guid userId);
}