namespace Core.Elasticsearch;

public class ElasticsearchConfiguration
{
  public string ConnectionString { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public string DefaultIndex { get; set; } = string.Empty;
  public int NumberOfShards { get; set; } = 1;
  public int NumberOfReplicas { get; set; } = 0;
}