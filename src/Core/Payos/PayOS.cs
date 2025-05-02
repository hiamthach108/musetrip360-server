namespace Core.Payos;

using Net.payOS;
using Net.payOS.Types;

public interface IPayOSService
{
  Task<CreatePaymentResult> CreatePayment(PaymentData data);
  Task<PaymentLinkInformation> GetPaymentInfo(long orderCode);
  Task<PaymentLinkInformation> CancelPayment(long orderCode, string reason);
  Task<string> ConfirmWebhook(string payload);
  WebhookData VerifyWebhookData(WebhookType data);
}

public class PayOSService : IPayOSService
{
  private readonly PayOS _payClient;

  public PayOSService(IConfiguration configuration)
  {
    var clientId = configuration["PayOS:ClientId"];
    var apiKey = configuration["PayOS:ApiKey"];
    var checksumKey = configuration["PayOS:ChecksumKey"];

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
    {
      throw new Exception("PayOS configuration is missing");
    }

    PayOS payOS = new PayOS(clientId, apiKey, checksumKey);

    _payClient = payOS;
  }


  public async Task<CreatePaymentResult> CreatePayment(PaymentData data)
  {
    return await _payClient.createPaymentLink(data);
  }

  public async Task<PaymentLinkInformation> GetPaymentInfo(long orderCode)
  {
    return await _payClient.getPaymentLinkInformation(orderCode);
  }

  public async Task<PaymentLinkInformation> CancelPayment(long orderCode, string reason)
  {
    return await _payClient.cancelPaymentLink(orderCode, reason);
  }

  public async Task<string> ConfirmWebhook(string payload)
  {
    return await _payClient.confirmWebhook(payload);
  }

  public WebhookData VerifyWebhookData(WebhookType data)
  {
    return _payClient.verifyPaymentWebhookData(data);
  }
}