using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceValidator.API.Data.Entities;

public class Booking
{
    [Column("BookingID")]
    public int ID { get; set; }
    public string Customer { get; set; } = string.Empty;
    public string CarrierCode { get; set; } = string.Empty;
    public int FlightNo { get; set; }
    public DateOnly FlightDate { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? InvoiceNumber { get; set; }
}
