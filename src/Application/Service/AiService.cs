namespace Application.Service;

using Application.DTOs.Ai;
using Application.DTOs.Search;
using Application.Shared.Type;
using Core.Elasticsearch;
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
  private readonly ISemanticSearchService _semanticSearchService;

  public AiService(ILLM llm, ISemanticSearchService semanticSearchService)
  {
    _llm = llm;
    _semanticSearchService = semanticSearchService;
  }

  public async Task<IActionResult> HandleChat(ChatReq req)
  {
    if (req.IsVector == true)
    {
      var semanticResult = await _semanticSearchService.SearchByQueryAsync(
        new SemanticSearchQuery
        {
          Query = req.Prompt,
          Page = 1,
          PageSize = 10,
          MinSimilarity = 0.7m,
          IncludeEmbeddings = false
        });

      var resultWithData = await _llm.CompleteWithDataAsync(req.Prompt, [.. semanticResult.Items.Cast<object>()]);

      return SuccessResp.Ok(new
      {
        data = resultWithData,
        relatedData = semanticResult.Items
      });
    }

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