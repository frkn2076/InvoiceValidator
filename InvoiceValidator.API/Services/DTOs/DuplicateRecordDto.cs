namespace InvoiceValidator.API.Services.DTOs;

public record DuplicateRecordDto(FlightInvoiceDto FlightInvoice,
                                 int NumberOfSeatsInvoiced,
                                 int NumberOfSeatsBooked);
