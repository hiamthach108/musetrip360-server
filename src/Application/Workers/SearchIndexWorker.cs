namespace Application.Workers;

using Application.DTOs.Search;
using Application.Service;
using Application.Shared.Constant;
using Application.Shared.Helpers;
using Core.Queue;

public class SearchIndexWorker : BackgroundService
{
  private readonly IQueueSubscriber _queueSubscriber;
  private readonly ILogger<SearchIndexWorker> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public SearchIndexWorker(
    IQueueSubscriber queueSubscriber,
    ILogger<SearchIndexWorker> logger,
    IServiceScopeFactory scopeFactory
  )
  {
    _logger = logger;
    _queueSubscriber = queueSubscriber;
    _scopeFactory = scopeFactory;
    _logger.LogInformation("SearchIndexWorker initialized");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("SearchIndexWorker started");

    // run task in background and subscribe to the queue
    var result = await _queueSubscriber.Subscribe(QueueConst.Indexing, async (message) =>
    {
      // Create a new scope for each message processing to ensure that the DbContext is disposed of properly
      // and to avoid any potential memory leaks
      using var scope = _scopeFactory.CreateScope();
      var indexService = scope.ServiceProvider.GetRequiredService<ISearchItemService>();

      try
      {
        var indexMsg = JsonHelper.DeserializeObject<IndexMessage>(message);
        if (indexMsg == null)
        {
          throw new Exception("Failed to deserialize indexMsg");
        }

        // Logic handling for indexMsg
        _logger.LogInformation("Processing indexMsg: {Id}, Type: {Type}, Action: {Action}",
          indexMsg.Id, indexMsg.Type, indexMsg.Action);
        switch (indexMsg.Action.ToLower())
        {
          case IndexConst.CREATE_ACTION:
            switch (indexMsg.Type)
            {
              case IndexConst.MUSEUM_TYPE:
                await indexService.IndexMuseumAsync(indexMsg.Id);
                break;
              case IndexConst.ARTIFACT_TYPE:
                await indexService.IndexArtifactAsync(indexMsg.Id);
                break;
              case IndexConst.EVENT_TYPE:
                await indexService.IndexEventAsync(indexMsg.Id);
                break;
              case IndexConst.TOUR_ONLINE_TYPE:
                await indexService.IndexTourOnlineAsync(indexMsg.Id);
                break;
              default:
                _logger.LogWarning("Unknown type: {Type} for indexMsg with Id: {Id}", indexMsg.Type, indexMsg.Id);
                break;
            }

            break;
          case IndexConst.DELETE_ACTION:
            await indexService.DeleteItemFromIndexAsync(indexMsg.Id);
            break;
          default:
            _logger.LogWarning("Unknown action: {Action} for indexMsg with Id: {Id}", indexMsg.Action, indexMsg.Id);
            break;
        }


        _logger.LogInformation("indexMsg handle successfully");
      }
      catch (Exception ex)
      {
        // Handle exception
        _logger.LogError(ex, "Failed to process indexMsg");
      }
    }, stoppingToken);

    if (!result.Success)
    {
      _logger.LogError("Failed to subscribe to indexMsg queue: {ErrorMessage}", result.ErrorMessage);
      throw new Exception($"Failed to subscribe to indexMsg queue: {result.ErrorMessage}");
    }
    _logger.LogInformation($"Subscribed to [{QueueConst.Indexing}] queue successfully");
  }
}