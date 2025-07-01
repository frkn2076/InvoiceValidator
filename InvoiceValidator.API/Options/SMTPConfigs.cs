using System.ComponentModel.DataAnnotations;

namespace InvoiceValidator.API.Options;

public class SMTPConfig
{
    [Required]
    [MinLength(3)]
    public required string Host { get; init; }

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public required int Port { get; init; }

    [Required]
    [EmailAddress]
    public required string FromEmail { get; init; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public required string FromPassword { get; init; }

    [Required]
    [EmailAddress]
    public required string ToEmail { get; init; }
}
