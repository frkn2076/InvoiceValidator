using InvoiceValidator.API.Services.DTOs;

namespace InvoiceValidator.API.Services.Contracts;

public interface IFlightInvoiceService
{
    (string InvoiceNumber, List<FlightInvoiceDto> FlightInvoices) ExtractInvoiceDetails(Stream pdfStream);
}
