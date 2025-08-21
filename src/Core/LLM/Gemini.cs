namespace Core.LLM;

using System.Text.Json;
using Application.Shared.Constant;
using Core.HttpClient;

public class GeminiSvc : ILLM
{
  private readonly string DEFAULT_PROMPT = "Bạn là một trợ lý AI cho MuseTrip360, một hệ thống quản lý bảo tàng hiện đại, có khả năng mở rộng. Nhiệm vụ chính của bạn là cung cấp thông tin, giải thích và hỗ trợ liên quan đến dự án MuseTrip360. MuseTrip360 là một hệ thống quản lý bảo tàng hiện đại, được thiết kế để tối ưu hóa hoạt động và nâng cao trải nghiệm của du khách. Nền tảng này hỗ trợ nhân viên bảo tàng quản lý hiện vật, sự kiện, vé và các chuyến tham quan ảo, đồng thời giúp du khách dễ dàng tiếp cận nội dung bảo tàng cùng với các đề xuất cá nhân hóa. MuseTrip360 hướng tới việc số hóa quy trình quản lý và làm phong phú thêm trải nghiệm văn hóa cho mọi người.";
  private readonly string DEFAULT_ERR_RESP = "Xin lỗi, tôi không thể xử lý yêu cầu của bạn ngay bây giờ. Vui lòng thử lại sau.";
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
              text = DEFAULT_PROMPT
            },
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

  public async Task<string> CompleteWithDataAsync(string prompt, List<object> data)
  {
    // convert data to JSON string
    var dataJson = JsonSerializer.Serialize(data);

    var promptWithData = $"{prompt}\n\n Và bạn có thể dựa trên những dữ liệu liên quan từ hệ thống. Dữ liệu liên quan: ```{dataJson}```";


    var response = await _httpClient.PostAsync<GeminiResponse>(
      $"/{AiModelConst.GEMINI_GENERATIVE}:generateContent",
      new
      {
        model = AiModelConst.GEMINI_GENERATIVE,
        contents = new
        {
          parts = new[] {
            new {
              text = DEFAULT_PROMPT
            },
            new {
              text = promptWithData
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
      contents = DEFAULT_ERR_RESP;
    }

    return contents;
  }

  public async Task<AudioResp?> GenerateAudioAsync(string text)
  {
    var response = await _httpClient.PostAsync<GeminiAudioResponse>(
      $"/models/{AiModelConst.GEMINI_TTS}:generateContent",
      new
      {
        model = AiModelConst.GEMINI_TTS,
        contents = new[] {
          new {
            parts = new[] {
              new {
                text
              }
            }
          }
        },
        generationConfig = new
        {
          responseModalities = new[] { "AUDIO" },
          speechConfig = new
          {
            voiceConfig = new
            {
              prebuiltVoiceConfig = new
              {
                voiceName = AiModelConst.DEFAULT_VOICE
              }
            }
          }
        }
      },
      _apiHeader
    );

    var audioParts = new List<AudioPart>();
    if (response != null && response.Candidates != null && response.Candidates.Count > 0)
    {
      foreach (var candidate in response.Candidates)
      {
        if (candidate.Content != null && candidate.Content.Parts != null && candidate.Content.Parts.Count > 0)
        {
          audioParts.AddRange(candidate.Content.Parts);
        }
      }
    }

    return audioParts.Count > 0 ? audioParts[0].InlineData : null;
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

public class GeminiAudioResponse
{
  public List<AudioCandidate> Candidates { get; set; } = [];
  public string ResponseId { get; set; } = "";
}

public class AudioCandidate
{
  public AudioContent Content { get; set; } = new();
  public string FinishReason { get; set; } = "";
  public int Index { get; set; } = 0;
}

public class AudioContent
{
  public List<AudioPart> Parts { get; set; } = [];
  public string Role { get; set; } = "";
}

public class AudioPart
{
  public AudioResp InlineData { get; set; } = null!;
}