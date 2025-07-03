namespace Application.Service;

using Application.DTOs.Ai;
using Application.Shared.Type;
using Core.LLM;
using Microsoft.AspNetCore.Mvc;


public interface IAiService
{
  Task<IActionResult> HandleChat(ChatReq req);
  Task<IActionResult> HandleEmbedding(ChatReq req);
}

public class AiService : IAiService
{
  private readonly ILLM _llm;

  public AiService(ILLM llm)
  {
    _llm = llm;
  }

  public async Task<IActionResult> HandleChat(ChatReq req)
  {
    var result = await _llm.CompleteAsync(req.Prompt);
    return SuccessResp.Ok(new
    {
      data = result
    });
  }

  public async Task<IActionResult> HandleEmbedding(ChatReq req)
  {
    var result = await _llm.EmbedAsync(req.Prompt);
    return SuccessResp.Ok(result);
  }
}