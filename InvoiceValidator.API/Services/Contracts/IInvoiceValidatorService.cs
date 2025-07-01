using InvoiceValidator.API.Services.DTOs;

namespace InvoiceValidator.API.Services.Contracts;

public interface IInvoiceValidatorService
{
    Task<InvoiceReportDto> ValidateInvoicesAsync(Stream pdfStream, CancellationToken cancellationToken = default);
}
