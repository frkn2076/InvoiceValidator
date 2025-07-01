using InvoiceValidator.API.Services.Contracts;
using InvoiceValidator.API.Services.DTOs;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace InvoiceValidator.API.Services.Implementations;

public partial class FlightInvoiceService : IFlightInvoiceService
{
    [GeneratedRegex(@"^\d+\s+\d+\s+\d{2}\.\d{2}\.\d{4}(?:\s+[^\s]+){7}(?:\s+[^\s]+)?$", RegexOptions.Compiled)]
    private static partial Regex InvoiceLineRegex();

    private const char _seperator = ' ';

    private static (string InvoiceNumber, List<FlightInvoiceDto> FlightInvoices) ExtractInvoiceDetails(Stream pdfStream)
    {

        static bool TryParseDateOnly(string data, out DateOnly result)
        {
            const string DateOnlyFormat = "dd.MM.yyyy";
            return DateOnly.TryParseExact(data, DateOnlyFormat, out result);
        }

        var allLines = ExtractAllLines(pdfStream);

        var invoiceNumber = GetInvoiceNumber(allLines);

        var flightInvoices = new List<FlightInvoiceDto>();

        foreach (var line in allLines)
        {
            if (InvoiceLineRegex().IsMatch(line.Trim()))
            {
                var parts = line.Split(_seperator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 9)
                {
                    var season = parts[0];
                    var VT = parts[1];
                    var carrierCode = parts[3];
                    var origin = parts[5];
                    var destination = parts[6];

                    if (TryParseDateOnly(parts[2], out var flightDate)
                        && int.TryParse(parts[4], out var flightNumber)
                        && int.TryParse(parts[7], out var numberOfSeats)
                        && decimal.TryParse(parts[8], out var pricePerSeat)
                        && decimal.TryParse(parts[9], out var totalPrice))
                    {
                        flightInvoices.Add(new FlightInvoiceDto(season,
                                                                VT,
                                                                flightDate,
                                                                carrierCode,
                                                                flightNumber,
                                                                origin,
                                                                destination,
                                                                numberOfSeats,
                                                                pricePerSeat,
                                                                totalPrice));
                    }
                }
            }
        }

        return (invoiceNumber, flightInvoices);
    }

    private static List<string> ExtractAllLines(Stream pdfStream)
    {
        var lines = new List<string>();

        using PdfDocument document = PdfDocument.Open(pdfStream);

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();

            if (words.Count == 0)
            {
                continue;
            }

            // Group words by their Y-position (same line)
            var groupedByLine = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 2))
                .OrderByDescending(g => g.Key); // Top to bottom

            foreach (var lineGroup in groupedByLine)
            {
                // Sort words left to right
                var sortedWords = lineGroup
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text);

                var lineText = string.Join(_seperator, sortedWords);
                lines.Add(lineText);
            }
        }

        return lines;
    }

    private static string GetInvoiceNumber(List<string> pdfLines)
    {
        const string InvoiceNumberTitle = "Nummer Seite Datum";
        var invoiceNumberTitleIndex = pdfLines.IndexOf(InvoiceNumberTitle);

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pdfLines.Count, invoiceNumberTitleIndex);

        var invoiceNumberLines = pdfLines[invoiceNumberTitleIndex + 1];

        var invoiceNumberParts = invoiceNumberLines.Split(_seperator);

        ArgumentOutOfRangeException.ThrowIfZero(invoiceNumberParts.Length);

        return invoiceNumberParts[0];
    }


    #region Explicit Interface Definitions

    (string InvoiceNumber, List<FlightInvoiceDto> FlightInvoices) IFlightInvoiceService.ExtractInvoiceDetails(Stream pdfStream) => ExtractInvoiceDetails(pdfStream);

    #endregion
}
