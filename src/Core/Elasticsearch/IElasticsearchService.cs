namespace Core.Elasticsearch;

public interface IElasticsearchService : IDisposable
{
  Task<bool> IndexExistsAsync(string indexName);
  Task<bool> CreateIndexAsync(string indexName, object? settings = null);
  Task<bool> DeleteIndexAsync(string indexName);
  Task<bool> IndexDocumentAsync<T>(string indexName, string id, T document) where T : class;
  Task<T?> GetDocumentAsync<T>(string indexName, string id) where T : class;
  Task<bool> DeleteDocumentAsync(string indexName, string id);
  Task<IEnumerable<T>> SearchAsync<T>(string indexName, string query, int from = 0, int size = 10) where T : class;
  Task<bool> BulkIndexAsync<T>(string indexName, IEnumerable<T> documents) where T : class;
  Task<bool> UpdateDocumentAsync<T>(string indexName, string id, T document) where T : class;
  Task<bool> RefreshIndexAsync(string indexName);
}