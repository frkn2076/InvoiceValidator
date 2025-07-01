# InvoiceValidator
**.Net 8**, **PDF**, **CSV**, **EF**

A .NET application to validate and process invoices by comparing invoice records against database entries.  
Generates summary reports and CSV attachments detailing unmatched, duplicate, and price-mismatched records.

## Features

- Validates invoices with multiple criteria:
  - Unmatched records
  - Duplicate invoices
  - Price discrepancies  
- Sends detailed email reports with CSV attachments for manual review.  
- Configurable via appsettings for SMTP and database settings.

## Getting Started

1. Clone the repo:  
   `git clone https://github.com/frkn2076/InvoiceValidator.git`

2. Configure your SMTP and database settings in `appsettings.json`.

3. Build and run the project.
