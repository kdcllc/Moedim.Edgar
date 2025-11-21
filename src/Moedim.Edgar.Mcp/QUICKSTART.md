# Quick Start Guide

## Installation

### From NuGet (Recommended)

```bash
# Install the MCP server as a global tool
dotnet tool install --global Moedim.Edgar.Mcp
```

### From Source

```bash
# Clone and build
git clone https://github.com/kdcllc/Moedim.Edgar.git
cd Moedim.Edgar
dotnet pack src/Moedim.Edgar.Mcp/Moedim.Edgar.Mcp.csproj -c Release
```

## Configuration for Claude Desktop

Add to your Claude Desktop MCP configuration file:

### Windows
Edit `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "edgar": {
      "command": "Moedim.Edgar.Mcp",
      "env": {
        "EDGAR_APP_NAME": "ClaudeDesktop",
        "EDGAR_APP_VERSION": "1.0.0",
        "EDGAR_EMAIL": "your.email@example.com"
      }
    }
  }
}
```

### macOS
Edit `~/Library/Application Support/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "edgar": {
      "command": "Moedim.Edgar.Mcp",
      "env": {
        "EDGAR_APP_NAME": "ClaudeDesktop",
        "EDGAR_APP_VERSION": "1.0.0",
        "EDGAR_EMAIL": "your.email@example.com"
      }
    }
  }
}
```

### Linux
Edit `~/.config/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "edgar": {
      "command": "Moedim.Edgar.Mcp",
      "env": {
        "EDGAR_APP_NAME": "ClaudeDesktop",
        "EDGAR_APP_VERSION": "1.0.0",
        "EDGAR_EMAIL": "your.email@example.com"
      }
    }
  }
}
```

## Configuration for VS Code

The `.vscode/mcp.json` file is already configured for local development:

```json
{
  "inputs": [],
  "servers": {
    "edgar": {
      "command": "dotnet",
      "args": ["run", "--project", "src/Moedim.Edgar.Mcp/Moedim.Edgar.Mcp.csproj"],
      "env": {
        "EDGAR_APP_NAME": "Moedim.Edgar.Mcp",
        "EDGAR_APP_VERSION": "1.0.0",
        "EDGAR_EMAIL": "user@example.com"
      }
    }
  }
}
```

## Example Queries

### Financial Analysis
```
"What was Apple's revenue trend over the last 4 quarters?"
"Show me Microsoft's total assets over time"
"Compare Tesla's quarterly net income for the past year"
```

### Filing Discovery
```
"Find all 8-K filings for Amazon in the last 6 months"
"Get the latest 10-K for Google"
"Show me recent insider trading activity (Form 4) for NVIDIA"
```

### Deep Dives
```
"Extract all data files from Apple's latest 10-K"
"Get the XBRL data from Microsoft's most recent quarterly report"
"Show me all exhibits from Tesla's latest 8-K filing"
```

## Available Tools Overview

### Company Data (4 tools)
- `get_cik_from_symbol` - Convert ticker to CIK
- `get_company_facts` - Get all financial facts
- `get_company_concept` - Get specific concept history
- `list_common_concepts` - Reference guide for concepts

### Filing Search (4 tools)
- `search_company_filings` - Advanced search with filters
- `get_latest_filings` - Quick access to recent filings
- `get_next_filings_page` - Pagination support
- `list_common_form_types` - Reference guide for forms

### Filing Details (5 tools)
- `get_filing_details` - Complete filing information
- `get_cik_from_filing` - Extract CIK from accession number
- `get_document_format_files` - Get all documents
- `get_data_files` - Get structured data files
- `get_primary_document_url` - Get main document URL

See [TOOLS.md](TOOLS.md) for comprehensive documentation.

## Environment Variables

### Required
- `EDGAR_APP_NAME` - Your application name (for SEC user agent)
- `EDGAR_APP_VERSION` - Your application version
- `EDGAR_EMAIL` - Your email address (SEC requirement)

### Optional
- `EDGAR_REQUEST_DELAY_MS` - Delay between requests (default: 100ms)
- `EDGAR_MAX_RETRY_COUNT` - Maximum retry attempts (default: 3)

## Common Use Cases

### 1. Financial Statement Analysis
```
1. Look up company: get_cik_from_symbol("AAPL")
2. Get all facts: get_company_facts(cik)
3. Analyze specific metrics: get_company_concept for key concepts
```

### 2. Event Monitoring
```
1. Search for 8-K filings: search_company_filings(symbol, filingType="8-K")
2. Review recent events: get_latest_filings(symbol)
3. Deep dive: get_filing_details(accessionNumber)
```

### 3. Data Extraction
```
1. Find filing: search_company_filings
2. Get data files: get_data_files(accessionNumber)
3. Access XBRL/JSON data for analysis
```

## Troubleshooting

### Server Not Starting
- Ensure .NET 10.0 SDK is installed
- Check environment variables are set correctly
- Verify email format is valid

### No Results
- CIK numbers should be 10 digits with leading zeros
- Stock symbols are case-insensitive
- Date formats should be YYYY-MM-DD

### Rate Limiting
- SEC enforces rate limits (10 requests per second)
- The server includes automatic retry with backoff
- Adjust `EDGAR_REQUEST_DELAY_MS` if needed

## Support & Resources

- **Documentation**: [TOOLS.md](TOOLS.md)
- **GitHub**: https://github.com/kdcllc/Moedim.Edgar
- **Issues**: https://github.com/kdcllc/Moedim.Edgar/issues
- **SEC EDGAR**: https://www.sec.gov/edgar

## License

MIT License - See [LICENSE](../../LICENSE) for details
