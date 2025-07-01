namespace InvoiceValidator.API.Services.DTOs;

public record FlightInvoiceDto(string Season,
                               string VT,
                               DateOnly FlightDate,
                               string CarrierCode,
                               int FlightNumber,
                               string Origin,
                               string Destination,
                               int NumberOfSeats,
                               decimal PricePerSeat,
                               decimal TotalPrice);
