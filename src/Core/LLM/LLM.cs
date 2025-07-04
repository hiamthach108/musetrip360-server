namespace Core.LLM;

public interface ILLM
{
  Task<string> CompleteAsync(string prompt, string? defaultResponse = null);
  Task CompleteStream(string prompt, Action<string> callback, string? defaultResponse = null);

  Task<List<float>> EmbedAsync(string text);
}