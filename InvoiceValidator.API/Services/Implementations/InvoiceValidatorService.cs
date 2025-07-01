using InvoiceValidator.API.Data;
using InvoiceValidator.API.Data.Entities;
using InvoiceValidator.API.Services.Contracts;
using InvoiceValidator.API.Services.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InvoiceValidator.API.Services.Implementations;

public class InvoiceValidatorService(InvoiceDbContext dbContext,
                                     IFlightInvoiceService flightInvoiceService) : IInvoiceValidatorService
{
    private async Task<InvoiceReportDto> ValidateInvoicesAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        var (invoiceNumber, flightInvoices) = flightInvoiceService.ExtractInvoiceDetails(pdfStream);

        var flightKeys = flightInvoices
            .Select(x => (x.FlightDate, x.CarrierCode, x.FlightNumber))
            .ToHashSet();

        var bookings = await GetBookingsAsync(flightKeys, cancellationToken);

        var bookingsPerFlight = bookings
            .GroupBy(x => new
            {
                x.FlightDate,
                x.CarrierCode,
                FlightNumber = x.FlightNo
            })
            .ToDictionary(x => x.Key, x => x.ToList());

        var unmatchedRecords = flightInvoices
            .Where(x => !bookingsPerFlight.ContainsKey(new
            {
                x.FlightDate,
                x.CarrierCode,
                x.FlightNumber
            }))
            .ToList();

        var priceMismatches = new List<PriceMismatchedRecordDto>();
        var duplicateInvoices = new List<DuplicateRecordDto>();

        var totalProcessedRecords = flightInvoices.Count;
        var successfulRecords = 0;

        foreach (var flightInvoice in flightInvoices)
        {
            if (bookingsPerFlight.TryGetValue(new { flightInvoice.FlightDate, flightInvoice.CarrierCode, flightInvoice.FlightNumber }, out var currentBookings))
            {
                var availableBookings = currentBookings.Where(x => string.IsNullOrEmpty(x.InvoiceNumber));
                var totalPriceOfAvailableBookings = availableBookings.Sum(x => x.Price);
                var numberOfAvailableBookings = availableBookings.Count();

                if (flightInvoice.TotalPrice == totalPriceOfAvailableBookings
                    && flightInvoice.NumberOfSeats == numberOfAvailableBookings)
                {
                    foreach (var availableBooking in availableBookings)
                    {
                        availableBooking.InvoiceNumber = invoiceNumber;
                    }

                    successfulRecords++;
                }
                else if (flightInvoice.TotalPrice < totalPriceOfAvailableBookings
                    && flightInvoice.NumberOfSeats < numberOfAvailableBookings)
                {
                    foreach (var availableBooking in availableBookings.Take(flightInvoice.NumberOfSeats))
                    {
                        availableBooking.InvoiceNumber = invoiceNumber;
                    }

                    successfulRecords++;
                }
                else
                {
                    if (flightInvoice.NumberOfSeats > numberOfAvailableBookings)
                    {
                        duplicateInvoices.Add(new DuplicateRecordDto(flightInvoice,
                                                                     flightInvoice.NumberOfSeats,
                                                                     numberOfAvailableBookings));
                    }

                    if (currentBookings.Any(x => x.Price != flightInvoice.PricePerSeat))
                    {
                        priceMismatches.Add(new PriceMismatchedRecordDto(flightInvoice,
                                                                         currentBookings.First().Price,
                                                                         flightInvoice.PricePerSeat));
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new InvoiceReportDto(totalProcessedRecords,
                                    successfulRecords,
                                    unmatchedRecords,
                                    duplicateInvoices,
                                    priceMismatches);
    }

    private async Task<List<Booking>> GetBookingsAsync(HashSet<(DateOnly FlightDate, string CarrierCode, int FlightNumber)> flightKeys,
                                                       CancellationToken cancellationToken = default)
    {
        var bookingFilter = FormatBookingFilter(flightKeys);

        string bookingQuery = $"""
                SELECT *
                FROM Bookings
                WHERE {bookingFilter.Length} != 0 AND {bookingFilter}
            """;

        return await dbContext.Bookings
            .FromSqlRaw(bookingQuery)
            .ToListAsync(cancellationToken);
    }

    private static string FormatBookingFilter(HashSet<(DateOnly FlightDate, string CarrierCode, int FlightNumber)> flightKeys)
    {
        return string.Join(" OR ",
            flightKeys.Select(x => $"(FlightDate = '{x.FlightDate:yyyy-MM-dd}' AND CarrierCode = '{x.CarrierCode}' AND FlightNo = {x.FlightNumber})"));
    }


    #region Explicit Interface Definitions

    Task<InvoiceReportDto> IInvoiceValidatorService.ValidateInvoicesAsync(Stream pdfStream, CancellationToken cancellationToken) => ValidateInvoicesAsync(pdfStream, cancellationToken);

    #endregion
}
