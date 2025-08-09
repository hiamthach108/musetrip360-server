namespace Core.LLM;

using Application.Shared.Constant;
using Core.HttpClient;

public class GeminiSvc : ILLM
{
  private readonly string DEFAULT_PROMPT = "";
  private Dictionary<string, string> _apiHeader = [];
  private readonly IHttpClientService _httpClient;

  public GeminiSvc(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiSvc> logger)
  {
    _apiHeader = new Dictionary<string, string>
    {
      { "x-goog-api-key", configuration["Gemini:ApiKey"] ?? "api_key" }
    };
    _httpClient = new HttpClientService("https://generativelanguage.googleapis.com/v1beta", httpClientFactory, logger);
  }


  public async Task<string> CompleteAsync(string prompt, string? defaultResponse = null)
  {
    var response = await _httpClient.PostAsync<GeminiResponse>(
      $"/{AiModelConst.GEMINI_GENERATIVE}:generateContent",
      new
      {
        model = AiModelConst.GEMINI_GENERATIVE,
        contents = new
        {
          parts = new[] {
            new {
              text = prompt
            }
          }
        }
      },
      _apiHeader
    );

    var contents = "";
    if (response != null && response.Candidates != null && response.Candidates.Count > 0)
    {
      // loop through parts
      foreach (var candidate in response.Candidates)
      {
        if (candidate.Content != null && candidate.Content.Parts != null && candidate.Content.Parts.Count > 0)
        {
          foreach (var part in candidate.Content.Parts)
          {
            contents += part.Text;
          }
        }
      }
    }

    if (string.IsNullOrEmpty(contents))
    {
      contents = defaultResponse ?? "";
    }

    return contents;
  }

  public Task CompleteStream(string prompt, Action<string> callback, string? defaultResponse = null)
  {
    throw new NotImplementedException();
  }

  public async Task<List<float>> EmbedAsync(string text)
  {
    var response = await _httpClient.PostAsync<GeminiEmbeddingResponse>(
      $"/{AiModelConst.GEMINI_EMBEDDING}:embedContent",
      new
      {
        model = AiModelConst.GEMINI_EMBEDDING,
        content = new
        {
          parts = new[] {
            new {
              text
            }
          }
        },
        taskType = "SEMANTIC_SIMILARITY",
        outputDimensionality = 768
      },
      _apiHeader
    );
    return response?.Embedding?.Values ?? [];
  }
}

public class GeminiEmbeddingResponse
{
  public EmbeddingData Embedding { get; set; } = new();
}

public class EmbeddingData
{
  public List<float> Values { get; set; } = [];
}

public class GeminiResponse
{
  public List<Candidate> Candidates { get; set; } = [];
  public string ResponseId { get; set; } = "";
}

public class Candidate
{
  public Content Content { get; set; } = new();
  public string FinishReason { get; set; } = "";
  public int Index { get; set; } = 0;
}

public class Content
{
  public List<Part> Parts { get; set; } = [];
  public string Role { get; set; } = "";
}

public class Part
{
  public string Text { get; set; } = "";
}