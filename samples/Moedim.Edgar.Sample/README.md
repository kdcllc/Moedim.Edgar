# Moedim.Edgar Sample Application

This sample application demonstrates how to use the Moedim.Edgar library to access SEC EDGAR data.

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
