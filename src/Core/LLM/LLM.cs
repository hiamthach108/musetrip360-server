namespace Core.LLM;

public interface ILLM
{
  Task<string> CompleteAsync(string prompt, string? defaultResponse = null);
  Task<string> CompleteWithDataAsync(string prompt, List<object> data);
  Task CompleteStream(string prompt, Action<string> callback, string? defaultResponse = null);

  Task<List<float>> EmbedAsync(string text);
  Task<AudioResp?> GenerateAudioAsync(string text);
}

public class AudioResp
{
  public string? MimeType { get; set; }
  public string? Data { get; set; }
}