using CsvHelper;
using InvoiceValidator.API.Services.Contracts;
using InvoiceValidator.API.Services.DTOs;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace InvoiceValidator.API.Services.Implementations;

public class ReportService(INotificationService notificationService) : IReportService
{
    private static readonly CultureInfo _euroCulture = CultureInfo.GetCultureInfo("fr-FR");

    private async Task SendReportAsync(InvoiceReportDto invoiceReport, CancellationToken cancellationToken = default)
    {
        const string EmailTitle = "Invoice Summary Report";

        var emailBody = ConstructEmailBody(invoiceReport);

        var attachments = ConstructAttachments(invoiceReport);

        await notificationService.SendEmailAsync(EmailTitle, emailBody, attachments, cancellationToken);
    }


    private static string ConstructEmailBody(InvoiceReportDto invoiceReport)
    {
        static string FormatEuroPrice(decimal price)
        {
            const string CurrencyFormat = "C";
            return price.ToString(CurrencyFormat, _euroCulture);
        }

        var emailText = new StringBuilder();

        emailText.AppendLine("Dear Customer,");
        emailText.AppendLine();
        emailText.AppendLine("Your recent invoice import has been completed. Below is the summary of the results:");
        emailText.AppendLine();

        emailText.AppendLine($"- Total records processed: {invoiceReport.NumberOfProccessedRecords}");
        emailText.AppendLine($"- Successful records: {invoiceReport.NumberOfSuccessfulRecords}");
        emailText.AppendLine($"- Invalid records: {invoiceReport.NumberOfProccessedRecords - invoiceReport.NumberOfSuccessfulRecords}");
        emailText.AppendLine();

        emailText.AppendLine("The invalid records are broken down as follows:");
        emailText.AppendLine($"1. Unmatched records: {invoiceReport.UnmatchedRecords.Count} (sum of prices: {FormatEuroPrice(invoiceReport.UnmatchedRecords.Sum(r => r.TotalPrice))})");
        emailText.AppendLine($"2. Duplicate invoices: {invoiceReport.DuplicateRecords.Count} (sum of prices: {FormatEuroPrice(invoiceReport.DuplicateRecords.Sum(x => x.FlightInvoice.TotalPrice))})");
        emailText.AppendLine($"3. Price mismatches: {invoiceReport.PriceMismatchedRecords.Count} (sum of differences: {FormatEuroPrice(invoiceReport.PriceMismatchedRecords.Sum(x => Math.Abs(x.PriceInvoiced - x.BookingPrice)))})");
        emailText.AppendLine();

        emailText.AppendLine("Details of these invalid records are attached as CSV files for manual review.");
        emailText.AppendLine();
        emailText.AppendLine("Kind regards,");
        emailText.AppendLine("Your Automated Import System");

        return emailText.ToString();
    }

    private static List<Attachment> ConstructAttachments(InvoiceReportDto invoiceReportDto)
    {
        static string ConvertToCSV<T>(List<T> invoices) where T : class
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(invoices);
            return writer.ToString();
        }

        static Attachment CreateCSVAttachment(string csvContent, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(csvContent);
            var stream = new MemoryStream(bytes);
            return new Attachment(stream, fileName, "text/csv");
        }

        var attachments = new List<Attachment>();

        if(invoiceReportDto.UnmatchedRecords.Count != 0)
        {
            var unmatchedRecordsContent = ConvertToCSV(invoiceReportDto.UnmatchedRecords);
            var unmatchedRecordsCSV = CreateCSVAttachment(unmatchedRecordsContent, "UnmatchedRecords.csv");
            attachments.Add(unmatchedRecordsCSV);
        }

        if(invoiceReportDto.DuplicateRecords.Count != 0)
        {
            var duplicateRecordsContent = ConvertToCSV(invoiceReportDto.DuplicateRecords);
            var duplicateRecordsCSV = CreateCSVAttachment(duplicateRecordsContent, "DuplicateRecords.csv");
            attachments.Add(duplicateRecordsCSV);
        }

        if(invoiceReportDto.PriceMismatchedRecords.Count != 0)
        {
            var priceMismatchRecordsContent = ConvertToCSV(invoiceReportDto.PriceMismatchedRecords);
            var priceMismatchRecordsCSV = CreateCSVAttachment(priceMismatchRecordsContent, "PriceMismatchedRecords.csv");
            attachments.Add(priceMismatchRecordsCSV);
        }

        return attachments;
    }


    #region Explicit Interface Definitions

    Task IReportService.SendReportAsync(InvoiceReportDto invoiceReport, CancellationToken cancellationToken) => SendReportAsync(invoiceReport, cancellationToken);

    #endregion
}
