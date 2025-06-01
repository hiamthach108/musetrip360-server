namespace Core.Elasticsearch;

using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using System.Reflection;

public class ElasticsearchService : IElasticsearchService
{
  private readonly ElasticsearchClient _client;
  private readonly ElasticsearchConfiguration _config;
  private readonly ILogger<ElasticsearchService> _logger;

  public ElasticsearchService(IOptions<ElasticsearchConfiguration> config, ILogger<ElasticsearchService> logger)
  {
    _config = config.Value;
    _logger = logger;

    var settings = new ElasticsearchClientSettings(new Uri(_config.ConnectionString))
        .Authentication(new BasicAuthentication(_config.Username, _config.Password))
        .DefaultIndex(_config.DefaultIndex)
        .RequestTimeout(TimeSpan.FromMinutes(2)); // Keep only essential settings

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
      _logger.LogError(ex, "Error checking if index {IndexName} exists", indexName);
      return false;
    }
  }

  public async Task<bool> CreateIndexAsync(string indexName, object? settings = null)
  {
    try
    {
      if (await IndexExistsAsync(indexName))
      {
        return true;
      }

      // Use simple index creation with basic settings for Vietnamese support
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
      );

      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating index {IndexName}", indexName);
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
      _logger.LogError(ex, "Error deleting index {IndexName}", indexName);
      return false;
    }
  }

  public async Task<bool> IndexDocumentAsync<T>(string indexName, string id, T document) where T : class
  {
    try
    {
      var response = await _client.IndexAsync(document, indexName, id);
      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error indexing document with id {Id} to index {IndexName}", id, indexName);
      return false;
    }
  }

  public async Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class
  {
    try
    {
      var response = await _client.GetAsync<T>(id, indexName);
      return response.IsValidResponse ? response.Source : null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error getting document with id {Id} from index {IndexName}", id, indexName);
      return null;
    }
  }

  public async Task<bool> DeleteDocumentAsync(string indexName, string id)
  {
    try
    {
      var response = await _client.DeleteAsync(indexName, id);
      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error deleting document with id {Id} from index {IndexName}", id, indexName);
      return false;
    }
  }

  public async Task<IEnumerable<T>> SearchAsync<T>(string indexName, string query, int from = 0, int size = 10) where T : class
  {
    try
    {
      var response = await _client.SearchAsync<T>(s => s
          .Indices(indexName)
          .From(from)
          .Size(size)
          .Query(q => q.QueryString(qs => qs.Query(query)))
      );

      return response.IsValidResponse ? response.Documents : Enumerable.Empty<T>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error searching in index {IndexName} with query {Query}", indexName, query);
      return Enumerable.Empty<T>();
    }
  }

  public async Task<bool> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents) where T : class
  {
    try
    {
      var documentList = documents?.ToList();
      if (documentList == null || !documentList.Any())
      {
        _logger.LogWarning("No documents provided for bulk indexing to index {IndexName}", indexName);
        return true;
      }

      _logger.LogInformation("Starting bulk indexing of {Count} documents to index {IndexName}",
          documentList.Count, indexName);

      // Ensure index exists before bulk indexing
      if (!await IndexExistsAsync(indexName))
      {
        _logger.LogInformation("Index {IndexName} does not exist, creating it", indexName);
        var indexCreated = await CreateIndexAsync(indexName);
        if (!indexCreated)
        {
          _logger.LogError("Failed to create index {IndexName} before bulk indexing", indexName);
          return false;
        }
      }

      // Validate documents before sending
      var validDocuments = documentList.Where(doc => doc != null).ToList();
      if (validDocuments.Count != documentList.Count)
      {
        _logger.LogWarning("Filtered out {FilteredCount} null documents from bulk indexing",
            documentList.Count - validDocuments.Count);
      }

      if (!validDocuments.Any())
      {
        _logger.LogWarning("No valid documents to index after filtering nulls");
        return false;
      }

      try
      {
        // Try simple IndexMany approach first
        var response = await _client.BulkAsync(b => b
            .Index(indexName)
            .IndexMany(validDocuments)
        );

        if (!response.IsValidResponse)
        {
          _logger.LogWarning("Bulk indexing failed, falling back to individual document indexing. Response: {Response}",
              response.DebugInformation);

          // Fallback to individual document indexing
          return await IndexDocumentsIndividually(indexName, validDocuments);
        }

        if (response.Errors)
        {
          var errorItems = response.ItemsWithErrors.ToList();
          _logger.LogWarning("Bulk indexing completed with {ErrorCount} errors out of {TotalCount} documents for index {IndexName}",
              errorItems.Count, validDocuments.Count, indexName);

          foreach (var errorItem in errorItems.Take(5)) // Log first 5 errors
          {
            _logger.LogError("Document indexing error: {Error} for document ID: {Id}",
                errorItem.Error?.Reason ?? "Unknown error", errorItem.Id);
          }

          // Return true if most documents were indexed successfully (less than 50% errors)
          return errorItems.Count < validDocuments.Count / 2;
        }

        _logger.LogInformation("Successfully bulk indexed {Count} documents to index {IndexName}",
            validDocuments.Count, indexName);
        return true;
      }
      catch (Exception bulkEx)
      {
        _logger.LogWarning(bulkEx, "Bulk indexing failed with exception, falling back to individual document indexing");
        return await IndexDocumentsIndividually(indexName, validDocuments);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error bulk indexing documents to index {IndexName}. Exception details: {Details}",
          indexName, ex.ToString());
      return false;
    }
  }

  private async Task<bool> IndexDocumentsIndividually<T>(string indexName, List<T> documents) where T : class
  {
    _logger.LogInformation("Starting individual document indexing for {Count} documents to index {IndexName}",
        documents.Count, indexName);

    var successCount = 0;
    var errorCount = 0;

    for (int i = 0; i < documents.Count; i++)
    {
      try
      {
        var document = documents[i];
        var documentId = GetDocumentId(document, i);

        var response = await _client.IndexAsync(document, indexName, documentId);

        if (response.IsValidResponse)
        {
          successCount++;
        }
        else
        {
          errorCount++;
          _logger.LogWarning("Failed to index document {Index}: {Error}", i, response.DebugInformation);
        }
      }
      catch (Exception ex)
      {
        errorCount++;
        _logger.LogError(ex, "Exception indexing document {Index}", i);
      }

      // Small delay to avoid overwhelming Elasticsearch
      if (i % 10 == 0 && i > 0)
      {
        await Task.Delay(50);
      }
    }

    _logger.LogInformation("Individual indexing completed. Success: {SuccessCount}, Errors: {ErrorCount}",
        successCount, errorCount);

    // Consider successful if at least 70% of documents were indexed
    return (double)successCount / documents.Count >= 0.7;
  }

  private string GetDocumentId<T>(T document, int fallbackIndex) where T : class
  {
    try
    {
      // Try to get ID from common property names
      var type = typeof(T);

      // Check for Id property
      var idProperty = type.GetProperty("Id");
      if (idProperty != null)
      {
        var value = idProperty.GetValue(document);
        if (value != null)
          return value.ToString() ?? $"doc_{fallbackIndex}";
      }

      // Check for _id property
      var _idProperty = type.GetProperty("_id");
      if (_idProperty != null)
      {
        var value = _idProperty.GetValue(document);
        if (value != null)
          return value.ToString() ?? $"doc_{fallbackIndex}";
      }

      // Generate ID based on index if no ID property found
      return $"doc_{fallbackIndex}_{Guid.NewGuid():N}";
    }
    catch
    {
      return $"doc_{fallbackIndex}_{Guid.NewGuid():N}";
    }
  }

  public async Task<bool> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents, int batchSize) where T : class
  {
    try
    {
      var documentList = documents?.ToList();
      if (documentList == null || !documentList.Any())
      {
        _logger.LogWarning("No documents provided for bulk indexing to index {IndexName}", indexName);
        return true;
      }

      _logger.LogInformation("Starting batched bulk indexing of {Count} documents to index {IndexName} with batch size {BatchSize}",
          documentList.Count, indexName, batchSize);

      // Ensure index exists before bulk indexing
      if (!await IndexExistsAsync(indexName))
      {
        _logger.LogInformation("Index {IndexName} does not exist, creating it", indexName);
        var indexCreated = await CreateIndexAsync(indexName);
        if (!indexCreated)
        {
          _logger.LogError("Failed to create index {IndexName} before bulk indexing", indexName);
          return false;
        }
      }

      // Validate documents before sending
      var validDocuments = documentList.Where(doc => doc != null).ToList();
      if (validDocuments.Count != documentList.Count)
      {
        _logger.LogWarning("Filtered out {FilteredCount} null documents from bulk indexing",
            documentList.Count - validDocuments.Count);
      }

      if (!validDocuments.Any())
      {
        _logger.LogWarning("No valid documents to index after filtering nulls");
        return false;
      }

      var totalBatches = (int)Math.Ceiling((double)validDocuments.Count / batchSize);
      var successfulBatches = 0;
      var totalErrors = 0;

      for (int i = 0; i < totalBatches; i++)
      {
        var batch = validDocuments.Skip(i * batchSize).Take(batchSize).ToList();
        _logger.LogInformation("Processing batch {BatchNumber}/{TotalBatches} with {BatchCount} documents",
            i + 1, totalBatches, batch.Count);

        try
        {
          var response = await _client.BulkAsync(b => b
              .Index(indexName)
              .IndexMany(batch)
          );

          if (!response.IsValidResponse)
          {
            _logger.LogError("Batch {BatchNumber} failed for index {IndexName}. Response: {Response}",
                i + 1, indexName, response.DebugInformation);

            if (response.TryGetOriginalException(out var originalException))
            {
              _logger.LogError(originalException, "Original exception during batch {BatchNumber} bulk indexing", i + 1);
            }
            continue;
          }

          if (response.Errors)
          {
            var errorItems = response.ItemsWithErrors.ToList();
            totalErrors += errorItems.Count;
            _logger.LogWarning("Batch {BatchNumber} completed with {ErrorCount} errors out of {BatchCount} documents",
                i + 1, errorItems.Count, batch.Count);

            foreach (var errorItem in errorItems.Take(3)) // Log first 3 errors per batch
            {
              _logger.LogError("Document indexing error in batch {BatchNumber}: {Error} for document ID: {Id}",
                  i + 1, errorItem.Error?.Reason ?? "Unknown error", errorItem.Id);
            }
          }

          successfulBatches++;
          _logger.LogInformation("Batch {BatchNumber}/{TotalBatches} completed successfully", i + 1, totalBatches);
        }
        catch (Exception batchEx)
        {
          _logger.LogError(batchEx, "Exception occurred during batch {BatchNumber} processing", i + 1);
        }

        // Small delay between batches to avoid overwhelming Elasticsearch
        if (i < totalBatches - 1)
        {
          await Task.Delay(100);
        }
      }

      var successRate = (double)successfulBatches / totalBatches;
      _logger.LogInformation("Batched bulk indexing completed. {SuccessfulBatches}/{TotalBatches} batches successful. Total errors: {TotalErrors}",
          successfulBatches, totalBatches, totalErrors);

      // Consider successful if at least 70% of batches succeeded
      return successRate >= 0.7;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error during batched bulk indexing to index {IndexName}. Exception details: {Details}",
          indexName, ex.ToString());
      return false;
    }
  }

  public async Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class
  {
    try
    {
      var response = await _client.UpdateAsync<T, T>(indexName, id, u => u.Doc(document));
      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating document with id {Id} in index {IndexName}", id, indexName);
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
      _logger.LogError(ex, "Error refreshing index {IndexName}", indexName);
      return false;
    }
  }

  public async Task<bool> TestConnectionAsync()
  {
    try
    {
      _logger.LogInformation("Testing Elasticsearch connection...");
      var response = await _client.PingAsync();

      if (response.IsValidResponse)
      {
        _logger.LogInformation("Elasticsearch connection test successful");
        return true;
      }
      else
      {
        _logger.LogError("Elasticsearch connection test failed. Response: {Response}", response.DebugInformation);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Exception during Elasticsearch connection test");
      return false;
    }
  }

  public void Dispose()
  {
    // ElasticsearchClient in the new library doesn't implement IDisposable
    // No cleanup needed
  }
}