namespace Core.Elasticsearch;

using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Elastic.Clients.Elasticsearch.Mapping;
using Application.DTOs.Search;
using Elastic.Clients.Elasticsearch.Core.Bulk;

public interface IVectorSearchService : IDisposable
{
  Task<bool> CreateVectorIndexAsync(string indexName, int vectorDimensions = 768);
  Task<bool> IndexExistsAsync(string indexName);
  Task<bool> DeleteIndexAsync(string indexName);
  Task<(IEnumerable<T> documents, long totalHits, IEnumerable<float> scores)> VectorSearchAsync<T>(string indexName, float[] queryVector, int from = 0, int size = 10, float minScore = 0.7f, string? additionalFilter = null) where T : class;
  Task<bool> IndexDocumentWithVectorAsync<T>(string indexName, string id, T document, float[] vector) where T : class;
  Task<bool> BulkIndexWithVectorsAsync<T>(string indexName, IEnumerable<(T document, float[] vector)> documentsWithVectors) where T : class;
  Task<bool> RefreshIndexAsync(string indexName);
  Task<bool> TestConnectionAsync();
}

public class VectorSearchService : IVectorSearchService
{
  private readonly ElasticsearchClient _client;
  private readonly ElasticsearchConfiguration _config;
  private readonly ILogger<VectorSearchService> _logger;

  public VectorSearchService(IOptions<ElasticsearchConfiguration> config, ILogger<VectorSearchService> logger)
  {
    _config = config.Value;
    _logger = logger;

    var settings = new ElasticsearchClientSettings(new Uri(_config.ConnectionString))
        .Authentication(new BasicAuthentication(_config.Username, _config.Password))
        .DefaultIndex(_config.DefaultIndex)
        .RequestTimeout(TimeSpan.FromMinutes(2));

    _client = new ElasticsearchClient(settings);
  }

  public async Task<bool> IndexExistsAsync(string indexName)
  {
    try
    {
      var response = await _client.Indices.ExistsAsync(indexName);
      return response.Exists;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error checking if vector index {IndexName} exists", indexName);
      return false;
    }
  }

  public async Task<bool> CreateVectorIndexAsync(string indexName, int vectorDimensions = 768)
  {
    try
    {
      if (await IndexExistsAsync(indexName))
      {
        _logger.LogInformation("Vector index {IndexName} already exists", indexName);
        return true;
      }

      _logger.LogInformation("Creating vector index {IndexName} with {VectorDimensions} dimensions", indexName, vectorDimensions);

      var response = await _client.Indices.CreateAsync(indexName, c => c
        .Settings(s => s
          .NumberOfShards(_config.NumberOfShards)
          .NumberOfReplicas(_config.NumberOfReplicas)
          .Analysis(a => a
            .Analyzers(an => an
              .Custom("vietnamese_analyzer", ca => ca
                .Tokenizer("standard")
                .Filter("lowercase", "asciifolding", "stop")
              )
            )
          )
        )
        .Mappings(m => m
          .Properties<SemanticSearchItemDto>(p => p
            .Keyword(k => k.Id)
            .Text(t => t.Title, td => td.Analyzer("vietnamese_analyzer"))
            .Keyword(k => k.Type)
            .Text(t => t.Description, td => td.Analyzer("vietnamese_analyzer"))
            .Text(t => t.SearchText, td => td.Analyzer("vietnamese_analyzer"))
            .DenseVector(dv => dv.Embedding, dvd => dvd.Dims(vectorDimensions).Similarity(DenseVectorSimilarity.Cosine))
            .Keyword(k => k.Tags)
          )
        )
      );

      if (response.IsValidResponse)
      {
        _logger.LogInformation("Successfully created vector index {IndexName}", indexName);
        return true;
      }
      else
      {
        _logger.LogError("Failed to create vector index {IndexName}: {Error}", indexName, response.DebugInformation);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating vector index {IndexName}", indexName);
      return false;
    }
  }

  public async Task<bool> DeleteIndexAsync(string indexName)
  {
    try
    {
      var response = await _client.Indices.DeleteAsync(indexName);
      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting vector index {IndexName}", indexName);
      return false;
    }
  }

  [Obsolete]
  public async Task<(IEnumerable<T> documents, long totalHits, IEnumerable<float> scores)> VectorSearchAsync<T>(
    string indexName,
    float[] queryVector,
    int from = 0,
    int size = 10,
    float minScore = 0.7f,
    string? additionalFilter = null) where T : class
  {
    try
    {
      _logger.LogDebug("Performing vector search in index {IndexName} with query vector of {Dimensions} dimensions",
        indexName, queryVector.Length);

      var searchRequestDescriptor = new SearchRequestDescriptor<T>()
        .Index(indexName)
        .From(from)
        .Size(size)
        .MinScore(minScore);

      if (!string.IsNullOrEmpty(additionalFilter))
      {
        searchRequestDescriptor = searchRequestDescriptor.Query(q => q
          .Bool(b => b
            .Must(m => m
              .Script(s => s
                .Script(ss => ss
                  .Source($"cosineSimilarity(params.query_vector, 'embedding') + 1.0")
                  .Params(p => p.Add("query_vector", queryVector))
                )
              )
            )
            .Filter(f => f
              .QueryString(qs => qs.Query(additionalFilter))
            )
          )
        );
      }
      else
      {
        searchRequestDescriptor = searchRequestDescriptor.Query(q => q
          .Script(s => s
            .Script(ss => ss
              .Source($"cosineSimilarity(params.query_vector, 'embedding') + 1.0")
              .Params(p => p.Add("query_vector", queryVector))
            )
          )
        );
      }

      var response = await _client.SearchAsync<T>(searchRequestDescriptor);

      if (!response.IsValidResponse)
      {
        _logger.LogError("Vector search failed for index {IndexName}: {Error}", indexName, response.DebugInformation);
        return (Enumerable.Empty<T>(), 0, Enumerable.Empty<float>());
      }

      var documents = response.Documents ?? Enumerable.Empty<T>();
      var totalHits = response.Total;
      var scores = response.Hits.Select(hit => (float)(hit.Score ?? 0f));

      _logger.LogDebug("Vector search returned {Count} documents with total hits: {TotalHits}",
        documents.Count(), totalHits);

      return (documents, totalHits, scores);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error performing vector search in index {IndexName}", indexName);
      return (Enumerable.Empty<T>(), 0, Enumerable.Empty<float>());
    }
  }

  public async Task<bool> IndexDocumentWithVectorAsync<T>(string indexName, string id, T document, float[] vector) where T : class
  {
    try
    {
      // Create a dynamic object that includes both the document properties and the vector
      var docWithVector = CreateDocumentWithVector(document, vector);

      var response = await _client.IndexAsync(docWithVector, indexName, id);

      if (response.IsValidResponse)
      {
        _logger.LogDebug("Successfully indexed document {Id} with vector in index {IndexName}", id, indexName);
        return true;
      }
      else
      {
        _logger.LogError("Failed to index document {Id} with vector in index {IndexName}: {Error}",
          id, indexName, response.DebugInformation);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error indexing document {Id} with vector in index {IndexName}", id, indexName);
      return false;
    }
  }

  public async Task<bool> BulkIndexWithVectorsAsync<T>(string indexName, IEnumerable<(T document, float[] vector)> documentsWithVectors) where T : class
  {
    try
    {
      var documentList = documentsWithVectors?.ToList();
      if (documentList == null || !documentList.Any())
      {
        _logger.LogWarning("No documents provided for bulk vector indexing to index {IndexName}", indexName);
        return true;
      }

      _logger.LogInformation("Starting bulk vector indexing of {Count} documents to index {IndexName}",
        documentList.Count, indexName);

      if (!await IndexExistsAsync(indexName))
      {
        _logger.LogInformation("Vector index {IndexName} does not exist, creating it", indexName);
        var indexCreated = await CreateVectorIndexAsync(indexName);
        if (!indexCreated)
        {
          _logger.LogError("Failed to create vector index {IndexName} before bulk indexing", indexName);
          return false;
        }
      }

      var bulkOperations = new List<IBulkOperation>();

      foreach (var (document, vector) in documentList)
      {
        if (document != null && vector != null && vector.Length > 0)
        {
          var docId = GetDocumentId(document);
          var docWithVector = CreateDocumentWithVector(document, vector);

          bulkOperations.Add(new BulkIndexOperation<object>(docWithVector)
          {
            Index = indexName,
            Id = docId
          });
        }
      }

      if (!bulkOperations.Any())
      {
        _logger.LogWarning("No valid documents to index after filtering");
        return false;
      }

      var response = await _client.BulkAsync(b => b
        .Index(indexName)
        .IndexMany(bulkOperations)
      );

      if (!response.IsValidResponse)
      {
        _logger.LogError("Bulk vector indexing failed for index {IndexName}: {Error}",
          indexName, response.DebugInformation);
        return false;
      }

      if (response.Errors)
      {
        var errorItems = response.ItemsWithErrors.ToList();
        _logger.LogWarning("Bulk vector indexing completed with {ErrorCount} errors out of {TotalCount} documents for index {IndexName}",
          errorItems.Count, documentList.Count, indexName);

        foreach (var errorItem in errorItems.Take(3))
        {
          _logger.LogError("Document vector indexing error: {Error} for document ID: {Id}",
            errorItem.Error?.Reason ?? "Unknown error", errorItem.Id);
        }

        return errorItems.Count < documentList.Count / 2;
      }

      _logger.LogInformation("Successfully bulk indexed {Count} documents with vectors to index {IndexName}",
        documentList.Count, indexName);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error bulk indexing documents with vectors to index {IndexName}", indexName);
      return false;
    }
  }

  public async Task<bool> RefreshIndexAsync(string indexName)
  {
    try
    {
      var response = await _client.Indices.RefreshAsync(indexName);
      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error refreshing vector index {IndexName}", indexName);
      return false;
    }
  }

  public async Task<bool> TestConnectionAsync()
  {
    try
    {
      _logger.LogInformation("Testing Elasticsearch connection for vector search...");
      var response = await _client.PingAsync();

      if (response.IsValidResponse)
      {
        _logger.LogInformation("Vector search Elasticsearch connection test successful");
        return true;
      }
      else
      {
        _logger.LogError("Vector search Elasticsearch connection test failed: {Error}", response.DebugInformation);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception during vector search Elasticsearch connection test");
      return false;
    }
  }

  private object CreateDocumentWithVector<T>(T document, float[] vector) where T : class
  {
    var documentType = typeof(T);
    var properties = documentType.GetProperties();

    var docDict = new Dictionary<string, object>();

    foreach (var prop in properties)
    {
      var value = prop.GetValue(document);
      if (value != null)
      {
        docDict[ToCamelCase(prop.Name)] = value;
      }
    }

    docDict["embedding"] = vector;

    return docDict;
  }

  private string GetDocumentId<T>(T document) where T : class
  {
    try
    {
      var type = typeof(T);
      var idProperty = type.GetProperty("Id");
      if (idProperty != null)
      {
        var value = idProperty.GetValue(document);
        if (value != null)
          return value.ToString() ?? Guid.NewGuid().ToString();
      }

      return Guid.NewGuid().ToString();
    }
    catch
    {
      return Guid.NewGuid().ToString();
    }
  }

  private string ToCamelCase(string input)
  {
    if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
      return input;

    return char.ToLower(input[0]) + input.Substring(1);
  }

  public void Dispose()
  {
    // ElasticsearchClient in the new library doesn't implement IDisposable

  }
}