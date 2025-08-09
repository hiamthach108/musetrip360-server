namespace Application.Controllers;

using Microsoft.AspNetCore.Mvc;
using Core.Elasticsearch;
using Application.DTOs.Search;

[ApiController]
[Route("/api/v1/vector-test")]
public class VectorSearchTestController : ControllerBase
{
  private readonly ILogger<VectorSearchTestController> _logger;
  private readonly IVectorSearchService _vectorSearchService;

  public VectorSearchTestController(ILogger<VectorSearchTestController> logger, IVectorSearchService vectorSearchService)
  {
    _logger = logger;
    _vectorSearchService = vectorSearchService;
  }

  [HttpPost("create-index")]
  public async Task<IActionResult> CreateTestIndex([FromBody] CreateIndexRequest request)
  {
    _logger.LogInformation("Creating test vector index: {IndexName}", request.IndexName);
    
    var result = await _vectorSearchService.CreateVectorIndexAsync(request.IndexName, request.VectorDimensions);
    
    if (result)
    {
      return Ok(new { success = true, message = $"Vector index '{request.IndexName}' created successfully" });
    }
    
    return BadRequest(new { success = false, message = $"Failed to create vector index '{request.IndexName}'" });
  }

  [HttpPost("bulk-index")]
  public async Task<IActionResult> BulkIndexTest([FromBody] BulkIndexRequest request)
  {
    _logger.LogInformation("Bulk indexing {Count} test documents to index: {IndexName}", 
      request.Documents.Count, request.IndexName);

    var documentsWithVectors = request.Documents.Select(doc => 
      ((object)new { id = doc.Id, title = doc.Title, content = doc.Content }, doc.Vector.ToArray())
    );

    var result = await _vectorSearchService.BulkIndexWithVectorsAsync(request.IndexName, documentsWithVectors);
    
    if (result)
    {
      return Ok(new { success = true, message = $"Successfully bulk indexed {request.Documents.Count} documents" });
    }
    
    return BadRequest(new { success = false, message = "Bulk indexing failed" });
  }

  [HttpPost("search")]
  public async Task<IActionResult> VectorSearch([FromBody] VectorSearchRequest request)
  {
    _logger.LogInformation("Vector search in index: {IndexName} with vector dimension: {Dimension}", 
      request.IndexName, request.QueryVector.Length);

    try
    {
      var (documents, totalHits, scores) = await _vectorSearchService.VectorSearchAsync<object>(
        request.IndexName, 
        request.QueryVector, 
        request.From, 
        request.Size, 
        request.MinScore
      );

      return Ok(new 
      { 
        success = true, 
        totalHits = totalHits,
        documents = documents,
        scores = scores 
      });
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Vector search failed for index: {IndexName}", request.IndexName);
      return BadRequest(new { success = false, message = "Vector search failed", error = ex.Message });
    }
  }

  [HttpPost("index-single")]
  public async Task<IActionResult> IndexSingleDocument([FromBody] SingleIndexRequest request)
  {
    _logger.LogInformation("Indexing single document with ID: {Id} to index: {IndexName}", 
      request.Id, request.IndexName);

    var document = new { id = request.Id, title = request.Title, content = request.Content };
    var result = await _vectorSearchService.IndexDocumentWithVectorAsync(request.IndexName, request.Id, document, request.Vector);
    
    if (result)
    {
      return Ok(new { success = true, message = $"Document '{request.Id}' indexed successfully" });
    }
    
    return BadRequest(new { success = false, message = $"Failed to index document '{request.Id}'" });
  }

  [HttpGet("index-exists/{indexName}")]
  public async Task<IActionResult> CheckIndexExists(string indexName)
  {
    var exists = await _vectorSearchService.IndexExistsAsync(indexName);
    return Ok(new { indexName = indexName, exists = exists });
  }

  [HttpDelete("index/{indexName}")]
  public async Task<IActionResult> DeleteIndex(string indexName)
  {
    _logger.LogInformation("Deleting vector index: {IndexName}", indexName);
    
    var result = await _vectorSearchService.DeleteIndexAsync(indexName);
    
    if (result)
    {
      return Ok(new { success = true, message = $"Vector index '{indexName}' deleted successfully" });
    }
    
    return BadRequest(new { success = false, message = $"Failed to delete vector index '{indexName}'" });
  }

  [HttpPost("refresh/{indexName}")]
  public async Task<IActionResult> RefreshIndex(string indexName)
  {
    var result = await _vectorSearchService.RefreshIndexAsync(indexName);
    
    if (result)
    {
      return Ok(new { success = true, message = $"Index '{indexName}' refreshed successfully" });
    }
    
    return BadRequest(new { success = false, message = $"Failed to refresh index '{indexName}'" });
  }

  [HttpGet("connection-test")]
  public async Task<IActionResult> TestConnection()
  {
    var result = await _vectorSearchService.TestConnectionAsync();
    return Ok(new { connected = result });
  }

  [HttpPost("generate-test-data")]
  public IActionResult GenerateTestData()
  {
    var random = new Random();
    var testDocuments = new List<TestDocument>();
    
    for (int i = 1; i <= 5; i++)
    {
      var vector = new float[768];
      for (int j = 0; j < 768; j++)
      {
        vector[j] = (float)(random.NextDouble() * 2.0 - 1.0);
      }
      
      testDocuments.Add(new TestDocument
      {
        Id = $"test-doc-{i}",
        Title = $"Test Document {i}",
        Content = $"This is test document number {i} with some sample content for vector search testing.",
        Vector = vector.ToList()
      });
    }

    return Ok(new 
    { 
      success = true, 
      message = "Generated 5 test documents with 768-dimensional vectors",
      documents = testDocuments 
    });
  }
}

public class CreateIndexRequest
{
  public string IndexName { get; set; } = string.Empty;
  public int VectorDimensions { get; set; } = 768;
}

public class BulkIndexRequest
{
  public string IndexName { get; set; } = string.Empty;
  public List<TestDocument> Documents { get; set; } = new();
}

public class VectorSearchRequest
{
  public string IndexName { get; set; } = string.Empty;
  public float[] QueryVector { get; set; } = Array.Empty<float>();
  public int From { get; set; } = 0;
  public int Size { get; set; } = 10;
  public float MinScore { get; set; } = 0.7f;
}

public class SingleIndexRequest
{
  public string IndexName { get; set; } = string.Empty;
  public string Id { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public float[] Vector { get; set; } = Array.Empty<float>();
}

public class TestDocument
{
  public string Id { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Content { get; set; } = string.Empty;
  public List<float> Vector { get; set; } = new();
}