using InvoiceValidator.API.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceValidator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class InvoiceController(IInvoiceValidatorService invoiceValidatorService,
                               IReportService reportService) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateAsync(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Empty file provided!");
        }

        using var stream = file.OpenReadStream();

        var invoiceReport = await invoiceValidatorService.ValidateInvoicesAsync(stream, cancellationToken);

        await reportService.SendReportAsync(invoiceReport, cancellationToken);

        return Ok();
    }
}