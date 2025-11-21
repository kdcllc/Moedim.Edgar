# Moedim Edgar CLI - SEC Filing Service

A command-line interface for accessing SEC EDGAR data using the Moedim.Edgar library.

## Overview

This CLI tool provides quick access to SEC EDGAR filing data through simple command-line commands. It leverages the same `Moedim.Edgar` library services used by applications but exposes them through an easy-to-use command-line interface.

## Features

- **Company Lookup**: Convert stock symbols to CIK numbers
- **Filing Search**: Search company filings with optional form type filtering
- **Filing Details**: Extract detailed filing metadata and document lists
- **Company Facts**: Retrieve all financial facts for a company
- **Concept Queries**: Get specific financial concept data (Revenues, Assets, etc.)
- **Latest Filings**: Monitor recent filings across all companies

## Prerequisites

- .NET 8.0 SDK or later

## Building the CLI

```bash
# Navigate to the sample directory
cd samples/Moedim.Edgar.Sample

# Build the project
dotnet build -c Release
```

## Running the CLI

```bash
# Run directly with dotnet run
dotnet run -- <command> [arguments]

# Or build and run the executable
dotnet build -c Release
dotnet run --no-build -c Release -- <command> [arguments]
```

## Commands

### Company Lookup

Get company information by ticker symbol or CIK.

```bash
dotnet run -- company <ticker|cik>
```

**Example:**
```bash
dotnet run -- company AAPL
dotnet run -- company 320193
```

**Output:**
```
Looking up company: AAPL...

Symbol/Ticker: AAPL
CIK: 0000320193
```

### Get Company Filings

Search for filings by company ticker or CIK, optionally filtered by form type.

```bash
dotnet run -- filings <ticker|cik> [form-type]
```

**Examples:**
```bash
# Get all filings for Microsoft
dotnet run -- filings MSFT

# Get only 10-K annual reports
dotnet run -- filings MSFT 10-K

# Get quarterly reports (10-Q)
dotnet run -- filings AAPL 10-Q

# Get current reports (8-K)
dotnet run -- filings TSLA 8-K
```

**Output:**
```
Fetching 10-K filings for MSFT...

Found 40 filing(s):

Form: 10-K
  Filed: 2024-07-30
  Description: Annual report [Section 13 and 15(d)...]
  URL: https://www.sec.gov/cgi-bin/browse-edgar?...

Form: 10-K
  Filed: 2023-07-27
  Description: Annual report [Section 13 and 15(d)...]
  URL: https://www.sec.gov/cgi-bin/browse-edgar?...
```

### Get Filing Details

Retrieve detailed information about a specific filing using its documents URL.

```bash
dotnet run -- filing-details <documents-url>
```

**Example:**
```bash
dotnet run -- filing-details "https://www.sec.gov/cgi-bin/browse-edgar?action=getcompany&CIK=0000320193&type=10-K&dateb=&owner=exclude&count=10&search_text="
```

**Output:**
```
Filing Details:
  Entity: Apple Inc. (CIK: 0000320193)
  Form Type: 10-K
  Accession: 0000320193-24-000123
  Filing Date: 2024-10-26
  Period of Report: 2024-09-28
  Accepted: 2024-10-26 18:04:22

Document Format Files (45):
  [1] 10-K - Annual report
      File: aapl-20240928.htm (10-K, 15,234,567 bytes)
  [2] EX-10.1 - Material Contract
      File: ex10-1.htm (EX-10.1, 123,456 bytes)
...

Data Files (12):
  [1] XBRL INSTANCE DOCUMENT
      File: aapl-20240928.xml (EX-101.INS)
  [2] XBRL TAXONOMY EXTENSION SCHEMA
      File: aapl-20240928.xsd (EX-101.SCH)
...
```

### Get Company Facts

Retrieve all financial facts (US-GAAP data) for a company by CIK.

```bash
dotnet run -- facts <cik>
```

**Example:**
```bash
dotnet run -- facts 320193
```

**Output:**
```
Fetching all facts for CIK 320193...

Entity: Apple Inc. (CIK: 320193)
Total Facts: 1,247

Sample Facts (first 20):
  - Net sales
    Tag: Revenues, Data Points: 156
    Latest: $391,035,000,000 (Period: 2024-09-28)
    
  - Total assets
    Tag: Assets, Data Points: 142
    Latest: $364,980,000,000 (Period: 2024-09-28)
...
```

### Get Specific Concept

Query a specific financial concept (e.g., Revenues, Assets, Liabilities) for a company.

```bash
dotnet run -- concept <cik> <tag>
```

**Examples:**
```bash
# Get revenue data for Apple
dotnet run -- concept 320193 Revenues

# Get assets for Microsoft
dotnet run -- concept 789019 Assets

# Get liabilities for Tesla
dotnet run -- concept 1318605 Liabilities
```

**Output:**
```
Fetching Revenues for CIK 320193...

Entity: Apple Inc. (CIK: 320193)
Concept: Net sales (Revenues)
Description: Amount of revenue recognized from goods sold...
Total Data Points: 156

Recent Filings:
  Period: 2024-09-28
    Value: $391,035,000,000
    Filed: 2024-10-31
    Form: 10-K
    
  Period: 2024-06-29
    Value: $85,777,000,000
    Filed: 2024-08-01
    Form: 10-Q
...
```

### Get Latest Filings

Get the most recent filings across all companies, optionally filtered by form type.

```bash
dotnet run -- latest [form-type]
```

**Examples:**
```bash
# Get all latest filings
dotnet run -- latest

# Get only latest 10-K filings
dotnet run -- latest 10-K

# Get latest 8-K current reports
dotnet run -- latest 8-K
```

**Output:**
```
Fetching latest filings (10-K)...

Latest 40 filing(s):

Company: Apple Inc.
  CIK: 0000320193
  Form: 10-K
  Filed: 2024-10-31
  Description: Annual report...

Company: Microsoft Corporation
  CIK: 0000789019
  Form: 10-K
  Filed: 2024-07-30
  Description: Annual report...
...
```

## Common Form Types

| Form | Description |
|------|-------------|
| 10-K | Annual report (comprehensive financial information) |
| 10-Q | Quarterly report (unaudited financial statements) |
| 8-K | Current report (material events or corporate changes) |
| DEF 14A | Proxy statement (annual meeting information) |
| Form 3 | Initial statement of beneficial ownership |
| Form 4 | Statement of changes in beneficial ownership |
| Form 5 | Annual statement of beneficial ownership |
| S-1 | Initial registration statement for new securities |
| 20-F | Annual report for foreign private issuers |

## Well-Known Companies

| Company | Symbol | CIK |
|---------|--------|-----|
| Apple Inc. | AAPL | 320193 |
| Microsoft Corporation | MSFT | 789019 |
| Tesla, Inc. | TSLA | 1318605 |
| Amazon.com, Inc. | AMZN | 1018724 |
| Alphabet Inc. (Google) | GOOGL | 1652044 |
| Meta Platforms, Inc. (Facebook) | META | 1326801 |

## Configuration

The CLI is configured in `Program.cs` with the following settings:

```csharp
services.AddSecEdgar(options =>
{
    options.AppName = "Moedim.Edgar.CLI";
    options.AppVersion = "1.0.0";
    options.Email = "cli@example.com";
    options.RequestDelay = TimeSpan.FromMilliseconds(100);
    options.MaxRetryCount = 3;
});
```

### SEC EDGAR Requirements

**User-Agent Header**: The SEC requires all automated requests to include:
- Application name
- Version number
- Contact email address

**Rate Limiting**: The SEC recommends:
- No more than 10 requests per second
- Implement delays between requests (configured as 100ms)
- Use retry logic for failed requests
- Respect `Retry-After` headers

## Error Handling

The CLI includes comprehensive error handling:

- Invalid commands display help information
- Missing required arguments show usage examples
- Failed API calls display error messages
- CIK validation ensures proper numeric format

## Development

### Project Structure

```
Moedim.Edgar.Sample/
├── Program.cs           # CLI implementation
├── GlobalUsings.cs      # Global using directives
├── README.md            # This file
└── Moedim.Edgar.Sample.csproj
```

### Adding New Commands

To add a new command:

1. Add a case to the switch statement in `Program.cs`
2. Create a static method to handle the command
3. Update the help text
4. Test the command

**Example:**
```csharp
case "newcommand":
    if (args.Length < 2)
    {
        Console.WriteLine("Error: Please provide required arguments");
        return;
    }
    await HandleNewCommand(service, args[1]);
    break;
```

## Additional Resources

- [Moedim.Edgar Library Documentation](../../README.md)
- [SEC EDGAR Developer Resources](https://www.sec.gov/developer)
- [XBRL Taxonomy Documentation](https://www.sec.gov/structureddata/osd-inline-xbrl.html)
- [Company Search Tool](https://www.sec.gov/edgar/searchedgar/companysearch.html)

## License

This CLI tool is part of the Moedim.Edgar project and follows the same license.
