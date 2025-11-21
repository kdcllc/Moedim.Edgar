# Moedim.Edgar

C# .NET library for accessing the Security Exchange Commission's EDGAR database with modern async/await patterns and dependency injection support.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/kdcllc/Moedim.Edgar/actions)
[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)](https://www.nuget.org/packages/Moedim.Edgar)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

![Stand With Israel](https://raw.githubusercontent.com/kdcllc/Moedim.Mapper/main/img/IStandWithIsrael.png)

> "Moedim" is a Hebrew word that translates to "feast" or "appointed time."
> "Appointed times" refers to HaShem's festivals in Vayikra/Leviticus 23rd.
> The feasts are "signals and signs" to help us know what is on the heart of HaShem.

## Features

- **Modern HTTP Client** - Uses IHttpClientFactory for efficient and reliable HTTP communication
- **Dependency Injection** - First-class support for Microsoft.Extensions.DependencyInjection
- **Type-Safe Models** - Strongly-typed models for SEC EDGAR data structures
- **Async/Await** - Fully asynchronous API throughout
- **Retry Logic** - Built-in retry mechanism for handling rate limiting and transient failures
- **Configurable** - Flexible options pattern for customizing behavior
- **Well-Documented** - Comprehensive XML documentation on all public APIs
- **Multi-Framework Support** - Targets .NET 8.0

## Hire me

Please send [email](mailto:info@kingdavidconsulting.com) if you consider to **hire me**.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Installation

```bash
dotnet add package Moedim.Edgar
```

## Quick Start

### 1. Configure Services

```csharp
using Moedim.Edgar.Extensions;

services.AddSecEdgar(options =>
{
    options.AppName = "YourCompany";
    options.AppVersion = "1.0.0";
    options.Email = "your@email.com";
    options.RequestDelay = TimeSpan.FromMilliseconds(100);
    options.MaxRetryCount = 3;
});
```

### 2. Use the Services

```csharp
using Moedim.Edgar.Models;

public class FinancialDataService
{
    private readonly ICompanyFactsService _companyFactsService;
    private readonly ICompanyConceptService _companyConceptService;

    public FinancialDataService(
        ICompanyFactsService companyFactsService,
        ICompanyConceptService companyConceptService)
    {
        _companyFactsService = companyFactsService;
        _companyConceptService = companyConceptService;
    }

    public async Task<CompanyFactsQuery> GetCompanyFactsAsync(int cik)
    {
        return await _companyFactsService.QueryAsync(cik);
    }

    public async Task<CompanyConceptQuery> GetRevenueDataAsync(int cik)
    {
        return await _companyConceptService.QueryAsync(cik, "Revenues");
    }
}
```

## Usage Examples

### Retrieve Company Facts

```csharp
// Get all facts for a company (e.g., Apple Inc. - CIK: 320193)
var facts = await companyFactsService.QueryAsync(320193);

// Access company information
Console.WriteLine($"Company: {facts.EntityName}");
Console.WriteLine($"CIK: {facts.Cik}");

// Iterate through facts
foreach (var fact in facts.Facts)
{
    Console.WriteLine($"Concept: {fact.Label}");
    Console.WriteLine($"Value: {fact.Value}");
}
```

### Query Specific Concepts

```csharp
// Get specific financial concept (e.g., Revenues)
var revenues = await companyConceptService.QueryAsync(320193, "Revenues");

// Access data points
foreach (var dataPoint in revenues.DataPoints)
{
    Console.WriteLine($"Period: {dataPoint.FiscalPeriod}");
    Console.WriteLine($"Value: {dataPoint.Value:C}");
    Console.WriteLine($"Filed: {dataPoint.Filed:d}");
}
```

## Configuration Options

```csharp
services.AddSecEdgar(options =>
{
    // Required: SEC requires identification
    options.AppName = "YourCompany";
    options.AppVersion = "1.0.0";
    options.Email = "your@email.com";

    // Optional: Delay between requests (default: 250ms)
    options.RequestDelay = TimeSpan.FromMilliseconds(100);

    // Optional: Delay after rate limit hit (default: 2 seconds)
    options.TimeoutDelay = TimeSpan.FromSeconds(1);

    // Optional: Maximum retry attempts (default: 10)
    options.MaxRetryCount = 3;

    // Optional: Override retry behavior (defaults shown)
    options.RetryCountOverride = 5; // total attempts including the initial call
    options.RetryDelay = TimeSpan.FromSeconds(1); // fallback when no Retry-After header is returned
    options.UseExponentialBackoff = true; // multiply delay after each retry
    options.RetryBackoffMultiplier = 1.5; // growth factor for exponential strategy
});
```

## Requirements

- .NET 8.0 or later
- C# 11 or later (for consuming projects)
- Visual Studio 2022 or Rider (for best IDE support)

## Project Structure

- **Moedim.Edgar** - Core library with services, models, and extensions
  - `Services/` - SEC EDGAR client and service implementations
  - `Models/` - Data models and query types
  - `Extensions/` - Dependency injection extensions

## API Documentation

### Services

- **ISecEdgarClient** - Low-level HTTP client for SEC EDGAR API
- **ICompanyFactsService** - Service for retrieving all company facts
- **ICompanyConceptService** - Service for querying specific financial concepts

### Models

- **CompanyFactsQuery** - Container for all company facts
- **CompanyConceptQuery** - Container for specific concept data
- **Fact** - Individual fact with label, value, and metadata
- **FactDataPoint** - Time-series data point for a fact
- **FiscalPeriod** - Enumeration of fiscal periods (Q1, Q2, Q3, Q4, FY)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

Built with:

- [IHttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [Microsoft.Extensions.DependencyInjection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Newtonsoft.Json](https://www.newtonsoft.com/json)

## Version History

See [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## Resources

- [SEC EDGAR API Documentation](https://www.sec.gov/edgar/sec-api-documentation)
- [SEC Company Search](https://www.sec.gov/cgi-bin/browse-edgar)

