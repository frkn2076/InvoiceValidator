using InvoiceValidator.API.Data;
using InvoiceValidator.API.Services.Contracts;
using InvoiceValidator.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace InvoiceValidator.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IFlightInvoiceService, FlightInvoiceService>();
        services.AddScoped<IInvoiceValidatorService, InvoiceValidatorService>();

        services.AddDbContext<InvoiceDbContext>(opts => opts.UseSqlServer(configuration.GetConnectionString(nameof(InvoiceDbContext))));
    }

    public static void ConfigureValidatedOptions<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        services
            .AddOptions<T>()
            .Bind(configuration.GetSection(typeof(T).Name))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
