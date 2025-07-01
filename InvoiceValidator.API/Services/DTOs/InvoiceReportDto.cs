namespace InvoiceValidator.API.Services.DTOs;

public record InvoiceReportDto(int NumberOfProccessedRecords,
                               int NumberOfSuccessfulRecords,
                               List<FlightInvoiceDto> UnmatchedRecords,
                               List<DuplicateRecordDto> DuplicateRecords,
                               List<PriceMismatchedRecordDto> PriceMismatchedRecords);
