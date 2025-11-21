# Moedim Edgar MCP Client Test Tool

A command-line test client for the Moedim.Edgar MCP (Model Context Protocol) server. This tool allows you to interact with and test all available MCP tools and prompts.

## Overview

This MCP client provides a simple interface to:
- List and discover available tools and prompts
- Call individual MCP tools with custom arguments
- Execute guided prompt workflows
- Run comprehensive test suites for all functionality

## Prerequisites

- .NET 8.0 SDK or later
- Built Moedim.Edgar.Mcp server (in Debug or Release mode)
- Valid email address for SEC EDGAR User-Agent compliance

### Building the MCP Server

Before using this client, you must first build the MCP server:

```bash
# Navigate to the MCP server directory
cd ../../src/Moedim.Edgar.Mcp

# Build the server
dotnet build

# The server will be built to (platform-specific):
# bin/Debug/net10.0/{runtime-id}/Moedim.Edgar.Mcp
# e.g., bin/Debug/net10.0/linux-x64/Moedim.Edgar.Mcp
```

## Building the Client

```bash
# Navigate to the client directory
cd samples/Moedim.Edgar.Mcp.Client

# Build the project
dotnet build -c Release
```

## Configuration

### Required Environment Variable

```bash
export EDGAR_EMAIL="your.email@example.com"
```

### Optional Environment Variables

```bash
# Custom MCP server path (if not in default location)
export MCP_SERVER_PATH="/path/to/Moedim.Edgar.Mcp"

# Or for platform-specific builds (e.g., linux-x64, osx-arm64, win-x64)
export MCP_SERVER_PATH="/path/to/Moedim.Edgar.Mcp/bin/Debug/net10.0/linux-x64/Moedim.Edgar.Mcp"
```

## Running the Client

```bash
dotnet run -- <command> [arguments]
```

## Commands

### Discovery Commands

#### `list-tools`
List all available MCP tools with descriptions and parameters.

```bash
dotnet run -- list-tools
```

**Output:**
```
Found 17 tools:

📦 get_cik_from_symbol
   Looks up a company's CIK (Central Index Key) from its trading symbol...
   Parameters:
     - symbol (required)

📦 get_company_facts
   Retrieves all financial facts for a company from SEC EDGAR...
   Parameters:
     - cik (required)
...
```

---

#### `list-prompts`
List all available MCP prompts with descriptions and arguments.

```bash
dotnet run -- list-prompts
```

**Output:**
```
Found 6 prompts:

📝 comprehensive-company-analysis
   Guides comprehensive company analysis using SEC filings...
   Arguments:
     - ticker (required): Company ticker symbol
     - focusAreas: Areas to focus on
     - timePeriod: Time period to analyze

📝 business-value-assessment
   Assesses business viability via 4 lenses...
   Arguments:
     - ticker (required): Company ticker symbol
...
```

---

### Tool Execution Commands

#### `call-tool`
Call a specific MCP tool with JSON arguments.

```bash
dotnet run -- call-tool <tool-name> '<json-arguments>'
```

**Examples:**

```bash
# Get CIK from symbol
dotnet run -- call-tool get_cik_from_symbol '{"symbol":"AAPL"}'

# Get company facts
dotnet run -- call-tool get_company_facts '{"cik":320193}'

# Search company filings
dotnet run -- call-tool search_company_filings '{"symbol":"MSFT","formType":"10-K","maxResults":5}'

# List common concepts
dotnet run -- call-tool list_common_concepts '{}'

# Get specific concept
dotnet run -- call-tool get_company_concept '{"cik":320193,"tag":"Revenues"}'

# Preview filing sections
dotnet run -- call-tool preview_filing_sections '{"accessionNumber":"0000320193-23-000077"}'

# Get filing sections
dotnet run -- call-tool get_filing_sections '{"accessionNumber":"0000320193-23-000077","anchorIds":["item_1a_risk_factors"],"merge":true}'
```

---

#### `get-prompt`
Get a specific prompt workflow with JSON arguments.

```bash
dotnet run -- get-prompt <prompt-name> '<json-arguments>'
```

**Examples:**

```bash
# Comprehensive company analysis for Apple
dotnet run -- get-prompt comprehensive-company-analysis '{"ticker":"AAPL"}'

# Business value assessment for Microsoft
dotnet run -- get-prompt business-value-assessment '{"ticker":"MSFT","formType":"10-K","compareToPeriodsAgo":"3"}'

# Compare peers
dotnet run -- get-prompt compare-peers '{"tickers":"AAPL,MSFT,GOOGL","comparisonFocus":"profitability"}'

# Query specific filing
dotnet run -- get-prompt query-filing '{"ticker":"TSLA","accessionNumber":"0001318605-24-000008","query":"What are the main supply chain risks?"}'

# Track changes in Risk Factors
dotnet run -- get-prompt track-changes '{"ticker":"NVDA","sectionFocus":"Risk Factors","formType":"10-K","periods":"3"}'

# Discover filings
dotnet run -- get-prompt discover-filings '{"searchGoal":"recent cybersecurity incident disclosures","tickers":"MSFT,GOOGL"}'
```

---

### Test Commands

#### `test-company-data`
Test all company data tools with a specific ticker.

```bash
dotnet run -- test-company-data [ticker]
```

**Default ticker:** AAPL

**Tests:**
1. `get_cik_from_symbol` - Convert ticker to CIK
2. `list_common_concepts` - List available financial concepts
3. `get_company_facts` - Retrieve all company facts
4. `get_company_concept` - Get Revenues concept data

**Example:**
```bash
dotnet run -- test-company-data TSLA
```

---

#### `test-filing-search`
Test all filing search tools with a specific ticker.

```bash
dotnet run -- test-filing-search [ticker]
```

**Default ticker:** MSFT

**Tests:**
1. `list_common_form_types` - List available SEC form types
2. `search_company_filings` - Search for 10-K filings
3. `get_latest_filings` - Get latest filings

**Example:**
```bash
dotnet run -- test-filing-search NVDA
```

---

#### `test-filing-details`
Test all filing details tools with a specific accession number.

```bash
dotnet run -- test-filing-details <accession-number>
```

**Tests:**
1. `get_cik_from_filing` - Extract CIK from accession number
2. `get_filing_details` - Get comprehensive filing details
3. `get_document_format_files` - List document files
4. `get_data_files` - List data files
5. `preview_filing_sections` - Preview filing sections

**Example:**
```bash
dotnet run -- test-filing-details 0000320193-23-000077
```

---

#### `test-all`
Run comprehensive tests of all MCP tools across all three categories.

```bash
dotnet run -- test-all
```

**Test Coverage:**
- Company Data Tools (with AAPL)
- Filing Search Tools (with MSFT)
- Filing Details Tools (with accession 0000320193-23-000077)

**Output:**
```
═══════════════════════════════════════════════
  Test Suite 1: Company Data Tools
═══════════════════════════════════════════════
Testing Company Data Tools with ticker: AAPL
...

═══════════════════════════════════════════════
  Test Suite 2: Filing Search Tools
═══════════════════════════════════════════════
Testing Filing Search Tools with ticker: MSFT
...

═══════════════════════════════════════════════
  Test Suite 3: Filing Details Tools
═══════════════════════════════════════════════
Testing Filing Details Tools with accession: 0000320193-23-000077
...

═══════════════════════════════════════════════
  All Tests Completed Successfully! ✓
═══════════════════════════════════════════════
```

---

## Available MCP Tools (17 total)

### Company Data Tools (4)
- `get_cik_from_symbol` - Convert stock symbol to CIK
- `get_company_facts` - Get all company financial facts
- `get_company_concept` - Get specific financial concept data
- `list_common_concepts` - List common XBRL concepts

### Filing Search Tools (4)
- `search_company_filings` - Search company filings by criteria
- `get_latest_filings` - Get recent company filings
- `get_next_filings_page` - Paginate search results
- `list_common_form_types` - List common SEC form types

### Filing Details Tools (9)
- `get_filing_details` - Get comprehensive filing details
- `get_cik_from_filing` - Extract CIK from accession number
- `get_document_format_files` - List document files
- `get_data_files` - List structured data files
- `get_filing_document` - Download complete filing content
- `preview_filing_sections` - Preview available sections
- `get_filing_sections` - Retrieve specific sections
- `get_primary_document_url` - Get primary document URL

## Available MCP Prompts (6 total)

### Analysis Workflows
- `comprehensive-company-analysis` - Complete company analysis workflow
- `business-value-assessment` - Four-lens viability assessment (Growth, Efficiency, Resilience, Tech)
- `compare-peers` - Multi-company comparison with differentiator matrix
- `query-filing` - Extract specific information from filings
- `track-changes` - Monitor disclosure evolution over time
- `discover-filings` - Discover relevant filings by criteria

## Common Use Cases

### 1. Analyze a Company

```bash
# Get basic company info
dotnet run -- test-company-data AAPL

# Get comprehensive analysis prompt
dotnet run -- get-prompt comprehensive-company-analysis '{"ticker":"AAPL","focusAreas":"growth strategy, competitive risks"}'
```

### 2. Search for Specific Filings

```bash
# Search for annual reports
dotnet run -- call-tool search_company_filings '{"symbol":"MSFT","formType":"10-K","maxResults":5}'

# Get recent quarterly reports
dotnet run -- call-tool search_company_filings '{"symbol":"TSLA","formType":"10-Q","maxResults":10}'
```

### 3. Extract Filing Content

```bash
# Preview sections first
dotnet run -- call-tool preview_filing_sections '{"accessionNumber":"0000320193-23-000077","tickerOrCik":"AAPL"}'

# Extract specific sections
dotnet run -- call-tool get_filing_sections '{"accessionNumber":"0000320193-23-000077","anchorIds":["item_1a_risk_factors","item_7_managements_discussion_analysis"],"merge":true}'
```

### 4. Compare Companies

```bash
# Get comparison prompt for tech giants
dotnet run -- get-prompt compare-peers '{"tickers":"AAPL,MSFT,GOOGL,META","comparisonFocus":"all","formType":"10-K"}'
```

### 5. Track Disclosure Changes

```bash
# Monitor risk factors over time
dotnet run -- get-prompt track-changes '{"ticker":"NVDA","sectionFocus":"Risk Factors","formType":"10-K","periods":"4"}'
```

## Error Handling

### MCP Server Not Found

```
Error: MCP server not found at: /path/to/server
Please build the MCP server first or set MCP_SERVER_PATH environment variable.
```

**Solution:**
```bash
# Build the MCP server first
cd ../../src/Moedim.Edgar.Mcp
dotnet build -c Debug

# Or set custom path
export MCP_SERVER_PATH="/custom/path/to/Moedim.Edgar.Mcp"
```

### Missing Email Configuration

The SEC requires a valid email in the User-Agent header.

**Solution:**
```bash
export EDGAR_EMAIL="your.email@example.com"
```

### JSON Argument Errors

Ensure JSON arguments are properly quoted and formatted.

**Incorrect:**
```bash
dotnet run -- call-tool get_cik_from_symbol {symbol:AAPL}
```

**Correct:**
```bash
dotnet run -- call-tool get_cik_from_symbol '{"symbol":"AAPL"}'
```

## Development

### Project Structure

```
Moedim.Edgar.Mcp.Client/
├── Program.cs                         # Main client implementation
├── GlobalUsings.cs                    # Global using directives
├── README.md                          # This file
└── Moedim.Edgar.Mcp.Client.csproj
```

### Adding New Test Scenarios

To add new test scenarios, create a new method following this pattern:

```csharp
static async Task TestNewFeature(McpClient client, string parameter)
{
    Console.WriteLine($"Testing New Feature with: {parameter}");
    Console.WriteLine();

    var result = await client.CallToolAsync(new McpCallToolRequest
    {
        Name = "tool_name",
        Arguments = new Dictionary<string, object>
        {
            ["param"] = parameter
        }
    });

    var resultText = ((McpTextContent)result.Content![0]).Text;
    Console.WriteLine($"   Result: {resultText}");
    Console.WriteLine();

    Console.WriteLine("✓ Test completed successfully!");
}
```

Then add a new command case in the main switch statement.

## MCP Configuration (mcp.json)

The Moedim.Edgar MCP server can be configured using the standard MCP `mcp.json` configuration file. This allows integration with MCP-compatible clients like Claude Desktop, VS Code Copilot, and others.

### Configuration File Locations

#### Claude Desktop

Place the `mcp.json` file in the Claude Desktop configuration directory:

- **Linux/macOS**: `~/.config/claude/mcp.json`
- **Windows**: `%APPDATA%\Claude\mcp.json`

#### VS Code Copilot

Place the `mcp.json` file in the VS Code user settings directory:

- **Linux**: `~/.config/Code/User/mcp.json`
- **macOS**: `~/Library/Application Support/Code/User/mcp.json`
- **Windows**: `%APPDATA%\Code\User\mcp.json`

### Platform-Specific Configuration Examples

This directory includes platform-specific example configurations:

- **`mcp.json`** - Linux configuration example
- **`mcp.macos.json`** - macOS configuration example
- **`mcp.windows.json`** - Windows configuration example

### Configuration Structure

```json
{
  "$schema": "https://modelcontextprotocol.io/schemas/mcp.json",
  "mcpServers": {
    "moedim-edgar": {
      "command": "dotnet",
      "args": [
        "/absolute/path/to/Moedim.Edgar.Mcp.dll"
      ],
      "env": {
        "EDGAR_APP_NAME": "MCP.Client",
        "EDGAR_APP_VERSION": "1.0.0",
        "EDGAR_EMAIL": "your.email@example.com",
        "EDGAR_REQUEST_DELAY_MS": "100",
        "EDGAR_MAX_RETRY_COUNT": "3"
      }
    }
  }
}
```

### Required Configuration Steps

1. **Build the MCP Server** (if not already built):
   ```bash
   cd src/Moedim.Edgar.Mcp
   dotnet build
   ```

2. **Locate the Server DLL**:
   - **Linux**: `bin/Debug/net10.0/linux-x64/Moedim.Edgar.Mcp.dll`
   - **macOS**: `bin/Debug/net10.0/osx-arm64/Moedim.Edgar.Mcp.dll`
   - **Windows**: `bin\Debug\net10.0\win-x64\Moedim.Edgar.Mcp.dll`

3. **Copy Example Configuration**:

   ```bash
   # For Linux
   cp samples/Moedim.Edgar.Mcp.Client/mcp.json ~/.config/claude/mcp.json

   # For macOS
   cp samples/Moedim.Edgar.Mcp.Client/mcp.macos.json ~/.config/claude/mcp.json

   # For Windows (PowerShell)
   Copy-Item samples\Moedim.Edgar.Mcp.Client\mcp.windows.json $env:APPDATA\Claude\mcp.json
   ```

4. **Customize the Configuration**:
   - Update the `args` path to the absolute path of your `Moedim.Edgar.Mcp.dll`
   - Set `EDGAR_EMAIL` to your email address (required by SEC EDGAR)
   - Optionally adjust `EDGAR_APP_NAME` and `EDGAR_APP_VERSION`
   - Optionally adjust rate limiting with `EDGAR_REQUEST_DELAY_MS` and `EDGAR_MAX_RETRY_COUNT`

### Environment Variables

#### Required

- **`EDGAR_EMAIL`**: Your email address (SEC EDGAR compliance requirement)

#### Optional

- **`EDGAR_APP_NAME`**: Application name for user-agent (default: "Moedim.Edgar.Mcp")
- **`EDGAR_APP_VERSION`**: Application version for user-agent (default: assembly version)
- **`EDGAR_REQUEST_DELAY_MS`**: Delay between requests in milliseconds (default: 100)
- **`EDGAR_MAX_RETRY_COUNT`**: Maximum retry attempts for failed requests (default: 3)

### Troubleshooting

#### Server Not Starting

**Issue**: MCP client can't connect to server

**Solutions**:

- Verify the DLL path is absolute and correct
- Ensure .NET runtime is installed and accessible
- Check that environment variables are set correctly
- Verify `EDGAR_EMAIL` is provided

#### Permission Denied

**Issue**: Server fails to execute

**Solutions**:

- On Linux/macOS, ensure the DLL directory has execute permissions
- Verify .NET runtime permissions
- Check file system access rights

#### Rate Limiting Errors

**Issue**: SEC EDGAR returns 429 (Too Many Requests)

**Solutions**:

- Increase `EDGAR_REQUEST_DELAY_MS` to 200 or higher
- Reduce concurrent request volume
- Follow SEC EDGAR fair access guidelines

## Additional Resources

- [Moedim.Edgar Library Documentation](../../README.md)
- [MCP Server Tools Documentation](../../src/Moedim.Edgar.Mcp/TOOLS.md)
- [Model Context Protocol Specification](https://modelcontextprotocol.io/)
- [SEC EDGAR Developer Resources](https://www.sec.gov/developer)

## License

This MCP client is part of the Moedim.Edgar project and follows the same license.
