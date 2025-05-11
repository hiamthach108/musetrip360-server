namespace Core.Mail;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
public interface IMailService
{
  Task SendEmailAsync(string email, string subject, string message);
}

public class MailService : IMailService
{
  private readonly string _smtpServer;
  private readonly int _smtpPort;
  private readonly string _smtpUsername;
  private readonly string _smtpPassword;

  public MailService(IConfiguration configuration)
  {
    _smtpServer = "smtp.gmail.com";
    _smtpPort = 587;
    _smtpUsername = configuration["SMTP:Email"] ?? "smtp_email";
    _smtpPassword = configuration["SMTP:Password"] ?? "smtp_password";
  }

  public async Task SendEmailAsync(string email, string subject, string message)
  {
    var mimeMessage = new MimeMessage();
    mimeMessage.From.Add(new MailboxAddress("Rescue & 360 Furniture", _smtpUsername));
    mimeMessage.To.Add(new MailboxAddress("Receiver Name", email));

    mimeMessage.Subject = subject;

    var bodyBuilder = new BodyBuilder
    {
      HtmlBody = message
    };

    mimeMessage.Body = bodyBuilder.ToMessageBody();

    using (var client = new SmtpClient())
    {
      await client.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
      await client.AuthenticateAsync(_smtpUsername, _smtpPassword);
      await client.SendAsync(mimeMessage);
      await client.DisconnectAsync(true);
    }
  }
}
