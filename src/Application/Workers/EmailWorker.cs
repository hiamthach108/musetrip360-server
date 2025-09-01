namespace Application.Workers;

using Application.DTOs.Email;
using Application.Shared.Constant;
using Application.Shared.Helpers;
using Core.Mail;
using Core.Queue;

public class EmailWorker : BackgroundService
{
  private readonly IQueueSubscriber _queueSubscriber;
  private readonly ILogger<EmailWorker> _logger;
  private readonly IServiceScopeFactory _scopeFactory;

  public EmailWorker(
    IQueueSubscriber queueSubscriber,
    ILogger<EmailWorker> logger,
    IServiceScopeFactory scopeFactory
  )
  {
    _logger = logger;
    _queueSubscriber = queueSubscriber;
    _scopeFactory = scopeFactory;
    _logger.LogInformation("EmailWorker initialized");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("EmailWorker started");

    var result = await _queueSubscriber.Subscribe(QueueConst.Email, async (message) =>
    {
      using var scope = _scopeFactory.CreateScope();
      var mailService = scope.ServiceProvider.GetRequiredService<IMailService>();

      try
      {
        var emailRequest = JsonHelper.DeserializeObject<SendEmailDto>(message);
        if (emailRequest == null)
        {
          throw new Exception("Failed to deserialize email request");
        }

        var htmlContent = await LoadEmailTemplate(emailRequest.Type, emailRequest.TemplateData);

        foreach (var recipient in emailRequest.Recipients)
        {
          await mailService.SendEmailAsync(recipient, emailRequest.Subject, htmlContent);
          _logger.LogInformation("Email sent successfully to {Recipient} with type {Type}", recipient, emailRequest.Type);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to process email request");
      }
    }, stoppingToken);

    if (!result.Success)
    {
      _logger.LogError("Failed to subscribe to email queue: {ErrorMessage}", result.ErrorMessage);
      throw new Exception($"Failed to subscribe to email queue: {result.ErrorMessage}");
    }
    _logger.LogInformation($"Subscribed to [{QueueConst.Email}] queue successfully");
  }

  private async Task<string> LoadEmailTemplate(string type, Dictionary<string, object> templateData)
  {
    try
    {
      var templatePath = Path.Combine("templates", $"{type}.html");

      if (!File.Exists(templatePath))
      {
        _logger.LogWarning("Template file not found: {TemplatePath}. Using default template.", templatePath);
        return GetDefaultTemplate(templateData);
      }

      var template = await File.ReadAllTextAsync(templatePath);

      foreach (var data in templateData)
      {
        template = template.Replace($"{{{{{data.Key}}}}}", data.Value?.ToString() ?? string.Empty);
      }

      return template;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error loading email template for type: {Type}", type);
      return GetDefaultTemplate(templateData);
    }
  }

  private string GetDefaultTemplate(Dictionary<string, object> templateData)
  {
    var content = templateData.ContainsKey("content") ? templateData["content"]?.ToString() : "Email notification";

    return $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8'>
        <title>Notification</title>
    </head>
    <body>
        <div style='font-family: Arial, sans-serif; padding: 20px;'>
            <h2>MuseTrip360 Notification</h2>
            <p>{content}</p>
            <hr>
            <p style='color: #666; font-size: 12px;'>
                This is an automated message from MuseTrip360. Please do not reply to this email.
            </p>
        </div>
    </body>
    </html>";
  }
}