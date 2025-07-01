using InvoiceValidator.API.Services.DTOs;

namespace InvoiceValidator.API.Services.Contracts;

public interface IReportService
{
    Task SendReportAsync(InvoiceReportDto invoiceReport, CancellationToken cancellationToken = default);
}
