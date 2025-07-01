using InvoiceValidator.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceValidator.API.Data;

public class InvoiceDbContext(DbContextOptions<InvoiceDbContext> opts) : DbContext(opts)
{
    public DbSet<Booking> Bookings { get; set; }
}
