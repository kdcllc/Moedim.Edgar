# Moedim.Edgar Sample Application

This comprehensive sample application demonstrates all features and services available in the Moedim.Edgar library for accessing SEC EDGAR data.

## Overview

The sample showcases complete usage of all six core services:

1. **Company Lookup Service** - Convert stock symbols to CIK numbers
2. **Company Facts Service** - Retrieve all financial facts for a company
3. **Company Concept Service** - Query specific financial concepts (Revenue, Assets, etc.)
4. **Edgar Search Service** - Search filings by company with filtering and pagination
5. **Edgar Latest Filings Service** - Get the latest filings across all companies
6. **Filing Details Service** - Extract detailed information from individual filings

## Features Demonstrated

### Company Lookup Service
- Lookup CIK by stock symbol (AAPL, MSFT, TSLA)
- Symbol to CIK conversion

### Company Facts Service
- Retrieve all financial facts for a company
- Filter facts by tag patterns
- Analyze fact data points
- Work with US-GAAP taxonomy data

### Company Concept Service
- Query specific financial concepts (Revenues, Assets, Liabilities)
- Analyze concept data points over time
- Filter by filing period
- Access historical financial data

### Edgar Search Service
- Search all filings for a company by symbol or CIK
- Filter by form type (10-K, 10-Q, 8-K, etc.)
- Apply date filters (PriorTo parameter)
- Control ownership filing inclusion/exclusion
- Pagination support (10, 40, 80, 100 results per page)
- Navigate through multiple pages of results

### Edgar Latest Filings Service
- Get latest filings across all companies
- Filter by form type (10-K, 10-Q, 8-K, etc.)
- Control ownership filing filters (Include, Exclude, Only)
- Various page size options
- Real-time latest filing discovery

### Filing Details Service
- Extract complete filing metadata
- Retrieve CIK from filing URLs
- List document format files
- List data files (XBRL, exhibits, etc.)
- Download specific documents
- Download XBRL instance documents
- Parse filing accession numbers, dates, and entity information

## Running the Sample

```bash
# Navigate to the sample directory
cd samples/Moedim.Edgar.Sample

# Run the application
dotnet run
```

## Configuration

The sample configures the SEC EDGAR client with:

```csharp
services.AddSecEdgar(options =>
{
    options.AppName = "Moedim.Edgar.Sample";
    options.AppVersion = "1.0.0";
    options.Email = "sample@example.com";
    options.RequestDelay = TimeSpan.FromMilliseconds(100);
    options.MaxRetryCount = 3;
    options.TimeoutDelay = TimeSpan.FromSeconds(30);
    options.UseExponentialBackoff = true;
    options.RetryBackoffMultiplier = 2;
});
```

### Important SEC EDGAR Requirements

**User-Agent Header**: The SEC requires all automated requests to include a proper User-Agent header with:
- Application name
- Version number
- Contact email

**Rate Limiting**: The SEC recommends:
- No more than 10 requests per second
- Implement delays between requests
- Use retry logic with exponential backoff
- Respect `Retry-After` headers

## Example Companies Used

The sample demonstrates queries for well-known companies:

| Company | Symbol | CIK |
|---------|--------|-----|
| Apple Inc. | AAPL | 320193 |
| Microsoft Corporation | MSFT | 789019 |
| Tesla, Inc. | TSLA | 1318605 |

## Common Form Types

| Form | Description |
|------|-------------|
| 10-K | Annual report |
| 10-Q | Quarterly report |
| 8-K | Current report (material events) |
| DEF 14A | Proxy statement |
| Form 3 | Initial statement of beneficial ownership |
| Form 4 | Statement of changes in beneficial ownership |
| Form 5 | Annual statement of beneficial ownership |

## Error Handling

Each example includes proper error handling:

```csharp
try
{
    var results = await service.QueryAsync(...);
    // Process results
}
catch (Exception ex)
{
    logger.LogError(ex, "Error message");
}
```

## Output Example

The sample produces comprehensive output showing:

- Service configuration details
- Request/response information
- Data structure exploration
- Pagination demonstration
- Error scenarios

## Learning Path

1. Start with **Company Lookup** to understand CIK resolution
2. Explore **Company Facts** to see all available data
3. Use **Company Concept** for specific financial metrics
4. Try **Edgar Search** for filing discovery
5. Check **Latest Filings** for recent submissions
6. Dive into **Filing Details** for document-level access

## Additional Resources

- [SEC EDGAR Developer Resources](https://www.sec.gov/developer)
- [XBRL Taxonomy Documentation](https://www.sec.gov/structureddata/osd-inline-xbrl.html)
- [Moedim.Edgar Documentation](../../README.md)

## Notes

- All examples include comprehensive error handling
- The sample respects SEC rate limiting guidelines
- Real-time data may vary from examples shown
- Some companies may have limited or no data for certain concepts
- XBRL documents are typically large XML files

## License

This sample is part of the Moedim.Edgar project and follows the same license.

## Features Demonstrated

- Dependency Injection setup
- Configuring SEC EDGAR services
- Retrieving company facts
- Querying specific financial concepts
- Error handling

## Running the Sample

```bash
cd samples/Moedim.Edgar.Sample
dotnet run
```

## Examples Included

### Company Facts Example

Demonstrates how to retrieve all facts for a company (Apple Inc.).

### Company Concept Example

Demonstrates how to query specific financial concepts like Revenues (Microsoft).

## Configuration

The sample uses the following configuration:

- AppName: Application name for SEC identification
- AppVersion: Application version
- Email: Contact email (required by SEC)
- Request Delay: 100ms between requests
- Max Retry Count: 3 attempts

## Important Notes

- The SEC requires AppName, AppVersion, and Email for API access
- Respect rate limits (10 requests per second maximum)
- Use appropriate delays between requests
