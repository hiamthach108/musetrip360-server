namespace Core.Elasticsearch;

using Microsoft.Extensions.Options;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

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
        .DefaultIndex(_config.DefaultIndex);

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
      var response = await _client.Indices.CreateAsync(indexName);
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
      var response = await _client.BulkAsync(b => b
          .Index(indexName)
          .IndexMany(documents)
      );

      return response.IsValidResponse;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error bulk indexing documents to index {IndexName}", indexName);
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

  public void Dispose()
  {
    // ElasticsearchClient in the new library doesn't implement IDisposable
    // No cleanup needed
  }
}