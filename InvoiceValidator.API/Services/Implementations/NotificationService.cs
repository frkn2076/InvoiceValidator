using InvoiceValidator.API.Options;
using InvoiceValidator.API.Services.Contracts;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace InvoiceValidator.API.Services.Implementations;

public class NotificationService(IOptions<SMTPConfig> smtpConfig) : INotificationService
{
    private async Task SendEmailAsync(string subject,
                                      string body,
                                      List<Attachment> attachments,
                                      CancellationToken cancellationToken = default)
    {
        using var smtp = new SmtpClient(smtpConfig.Value.Host, smtpConfig.Value.Port)
        {
            Credentials = new NetworkCredential(smtpConfig.Value.FromEmail, smtpConfig.Value.FromPassword),
            EnableSsl = true,
            UseDefaultCredentials = false
        };

        using var mail = new MailMessage(smtpConfig.Value.FromEmail, smtpConfig.Value.ToEmail, subject, body);

        foreach (var attachment in attachments)
        {
            mail.Attachments.Add(attachment);
        }

        await smtp.SendMailAsync(mail, cancellationToken);
    }


    #region Explicit Interface Definitions
    
    Task INotificationService.SendEmailAsync(string subject, string body, List<Attachment> attachments, CancellationToken cancellationToken) => SendEmailAsync(subject, body, attachments, cancellationToken);

    #endregion
}
