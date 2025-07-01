namespace InvoiceValidator.API.Services.DTOs;

public record PriceMismatchedRecordDto(FlightInvoiceDto FlightInvoice,
                                       decimal BookingPrice,
                                       decimal PriceInvoiced);
