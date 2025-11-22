# Moedim.Edgar.Mcp - MCP Server for SEC EDGAR Data

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/kdcllc/Moedim.Edgar/actions)
[![NuGet](https://img.shields.io/badge/nuget-v1.0.0-blue)](https://www.nuget.org/packages/Moedim.Edgar.Mcp)
[![License](https://img.shields.io/badge/license-MIT-green)](../../LICENSE)

A Model Context Protocol (MCP) server that provides AI assistants with access to SEC EDGAR filings and financial data. Built with the [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) C# SDK and the [Moedim.Edgar](https://www.nuget.org/packages/Moedim.Edgar) library.

## Features

### Core EDGAR Integration
- **SEC EDGAR Integration** - Direct access to Securities and Exchange Commission EDGAR database
- **Company Facts** - Query comprehensive company financial facts and filings
- **Financial Concepts** - Access specific financial concepts (revenues, assets, etc.)
- **Company Lookup** - Convert ticker symbols to CIK numbers
- **Filing Search** - Search company filings with filters
- **Latest Filings** - Retrieve the most recent SEC filings across all companies

### Document & Content Management
- **📄 Document Download** - Download complete SEC filings as clean Markdown or HTML
- **✂️ Section Extraction** - Parse and extract specific sections from filings (Risk Factors, MD&A, etc.)
- **🔍 Section Preview** - Browse filing sections before downloading full content
- **📋 Filing Details** - Get comprehensive metadata about specific filings
- **🔗 Data Files** - Access structured data files (XML, XBRL, JSON) associated with filings

### Performance & Architecture
- **💾 Built-in Caching** - Automatic file-based caching to reduce API calls and improve performance
- **🤖 Guided Workflows** - 6 pre-built prompts for common financial analysis patterns
- **Type-Safe** - Built on strongly-typed models with full async/await support
- **Cross-Platform** - Self-contained packages for Windows, macOS, and Linux
- **Retry Logic** - Built-in retry mechanism for handling rate limiting and transient failures

## Installation

### From NuGet.org

Once published, you can add the MCP server to your preferred IDE configuration:

#### Visual Studio Code

Create or update `.vscode/mcp.json` in your workspace:

```json
{
  "servers": {
    "Moedim.Edgar.Mcp": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "Moedim.Edgar.Mcp",
        "--version",
        "1.0.0",
        "--yes"
      ]
    }
  }
}
```

#### Visual Studio

Create or update `.mcp.json` in your solution directory:

```json
{
  "servers": {
    "Moedim.Edgar.Mcp": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "Moedim.Edgar.Mcp",
        "--version",
        "1.0.0",
        "--yes"
      ]
    }
  }
}
```

## Local Development and Testing

To test this MCP server from source code without using a built package:

### 1. Configure Local Testing

Create or update `.vscode/mcp.json` in the workspace root:

```json
{
  "servers": {
    "Moedim.Edgar.Mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "src/Moedim.Edgar.Mcp/Moedim.Edgar.Mcp.csproj"
      ]
    }
  }
}
```

### 2. Test with GitHub Copilot

Once configured, you can interact with the MCP server through GitHub Copilot Chat:

- "Get company facts for Apple (CIK: 320193)"
- "What are the revenues for Microsoft?"
- "Show me recent filings for Tesla"

## Publishing to NuGet.org

### 1. Pack the Project

```bash
dotnet pack src/Moedim.Edgar.Mcp/Moedim.Edgar.Mcp.csproj -c Release
```

This creates platform-specific packages for:
- `win-x64`, `win-arm64`
- `osx-arm64`
- `linux-x64`, `linux-arm64`, `linux-musl-x64`

### 2. Publish to NuGet

```bash
dotnet nuget push bin/Release/*.nupkg \
  --api-key <your-api-key> \
  --source https://api.nuget.org/v3/index.json
```

**Important**: Publish all `.nupkg` files to ensure every supported platform can run the MCP server.

### Testing Before Production

Test the publishing flow using the NuGet test environment at [int.nugettest.org](https://int.nugettest.org/):

```bash
dotnet nuget push bin/Release/*.nupkg \
  --api-key <your-test-api-key> \
  --source https://apiint.nugettest.org/v3/index.json
```

## Available Tools

The MCP server provides 17 tools organized into functional categories:

### Company Information (3 tools)
- `get_company_facts` - Retrieve all financial facts for a company by CIK
- `get_company_concept` - Get specific financial concept data (e.g., Revenues, Assets)
- `get_cik_from_symbol` - Convert ticker symbol to CIK number

### Filing Search & Discovery (3 tools)
- `search_company_filings` - Search filings by company symbol or CIK with filters
- `get_latest_filings` - Retrieve the most recent filings across all companies
- `get_filing_details` - Get comprehensive metadata about a specific filing

### Document Retrieval (3 tools)
- `get_filing_document` - Download complete filing as Markdown or HTML
- `preview_filing_sections` - List available sections in a filing
- `get_filing_sections` - Extract specific sections from a filing

### Filing Metadata (2 tools)
- `get_cik_from_filing` - Extract CIK from a filing document URL
- `get_data_files` - List structured data files (XML, XBRL, JSON) for a filing

See [TOOLS.md](TOOLS.md) for detailed documentation of all available tools.

## Technical Details

### Platform Support

The MCP server is built as a self-contained application and does not require the .NET runtime on the target machine. It's pre-compiled for the following platforms:

- Windows x64
- Windows ARM64
- macOS ARM64 (Apple Silicon)
- Linux x64
- Linux ARM64
- Linux musl x64 (Alpine Linux)

### Requirements

- **Development**: .NET 10.0 SDK or later
- **Runtime**: None (self-contained deployment)

## Project Structure

```
Moedim.Edgar.Mcp/
├── .mcp/
│   └── server.json          # MCP server metadata
├── Tools/
│   └── RandomNumberTools.cs # MCP tools implementation
├── Program.cs               # Application entry point
├── README.md                # This file
└── Moedim.Edgar.Mcp.csproj  # Project configuration
```

## More Information

### MCP Resources

- [Official MCP Documentation](https://modelcontextprotocol.io/)
- [MCP Protocol Specification](https://spec.modelcontextprotocol.io/)
- [MCP GitHub Organization](https://github.com/modelcontextprotocol)

### .NET MCP Resources

- [Build MCP Server with .NET Guide](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server)
- [MCP .NET Samples](https://github.com/microsoft/mcp-dotnet-samples)
- [ModelContextProtocol NuGet Package](https://www.nuget.org/packages/ModelContextProtocol)

### IDE Documentation

- [Use MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/chat/mcp-servers)
- [Use MCP servers in Visual Studio](https://learn.microsoft.com/visualstudio/ide/mcp-servers)

### Moedim.Edgar Library

- [Moedim.Edgar on NuGet](https://www.nuget.org/packages/Moedim.Edgar)
- [GitHub Repository](https://github.com/kdcllc/Moedim.Edgar)
- [SEC EDGAR API Documentation](https://www.sec.gov/edgar/sec-api-documentation)

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.

## Acknowledgments

Built with:
- [ModelContextProtocol](https://www.nuget.org/packages/ModelContextProtocol) - MCP C# SDK
- [Moedim.Edgar](https://www.nuget.org/packages/Moedim.Edgar) - SEC EDGAR data library
- [Microsoft.Extensions.Hosting](https://www.nuget.org/packages/Microsoft.Extensions.Hosting) - .NET Generic Host

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

If you find this project helpful, please give it a ⭐ on [GitHub](https://github.com/kdcllc/Moedim.Edgar)!

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)
