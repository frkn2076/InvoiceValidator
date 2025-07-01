using System.Net.Mail;

namespace InvoiceValidator.API.Services.Contracts;

public interface INotificationService
{
    Task SendEmailAsync(string subject,
                        string body,
                        List<Attachment> attachments,
                        CancellationToken cancellationToken = default);
}
