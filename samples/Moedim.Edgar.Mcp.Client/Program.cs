using System.Diagnostics;
using System.Text;

Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine("  Moedim Edgar MCP Server Test Tool");
Console.WriteLine("═══════════════════════════════════════════════");
Console.WriteLine();

if (args.Length == 0)
{
    ShowHelp();
    return;
}

var command = args[0].ToLower();

try
{
    // Get MCP server path from environment or use default
    var mcpServerPath = Environment.GetEnvironmentVariable("MCP_SERVER_PATH")
        ?? Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "src", "Moedim.Edgar.Mcp", "bin", "Debug", "net10.0", "Moedim.Edgar.Mcp");

    if (!File.Exists(mcpServerPath) && !File.Exists(mcpServerPath + ".dll"))
    {
        Console.WriteLine($"Error: MCP server not found at: {mcpServerPath}");
        Console.WriteLine("Please build the MCP server first or set MCP_SERVER_PATH environment variable.");
        Console.WriteLine();
        Console.WriteLine("To build the server:");
        Console.WriteLine("  cd ../../src/Moedim.Edgar.Mcp");
        Console.WriteLine("  dotnet build");
        return;
    }

    switch (command)
    {
        case "start-server":
            await StartMcpServer(mcpServerPath);
            break;

        case "test-stdio":
            await TestStdioConnection(mcpServerPath);
            break;

        case "run-sample":
            await RunSampleInteraction(mcpServerPath, args.Length > 1 ? args[1] : "AAPL");
            break;

        default:
            Console.WriteLine($"Unknown command: {command}");
            ShowHelp();
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Stack trace:");
    Console.WriteLine(ex.StackTrace);
}

static void ShowHelp()
{
    Console.WriteLine("Usage: dotnet run -- <command> [arguments]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  start-server                  Start the MCP server in interactive mode");
    Console.WriteLine("  test-stdio                    Test stdio connection to the MCP server");
    Console.WriteLine("  run-sample [ticker]           Run a sample interaction (default: AAPL)");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  dotnet run -- start-server");
    Console.WriteLine("  dotnet run -- test-stdio");
    Console.WriteLine("  dotnet run -- run-sample MSFT");
    Console.WriteLine();
    Console.WriteLine("Environment Variables:");
    Console.WriteLine("  MCP_SERVER_PATH    Path to the MCP server executable");
    Console.WriteLine("  EDGAR_EMAIL        Email for SEC EDGAR User-Agent (required)");
    Console.WriteLine();
    Console.WriteLine("Note:");
    Console.WriteLine("  This is a simple test client that demonstrates stdio communication");
    Console.WriteLine("  with the MCP server. For full MCP client functionality, use an");
    Console.WriteLine("  MCP-compatible client like Claude Desktop or VS Code Copilot.");
    Console.WriteLine();
    Console.WriteLine("MCP Server Tools:");
    Console.WriteLine("  - get_cik_from_symbol         Convert ticker to CIK");
    Console.WriteLine("  - get_company_facts           Get all company facts");
    Console.WriteLine("  - get_company_concept         Get specific concept data");
    Console.WriteLine("  - list_common_concepts        List available concepts");
    Console.WriteLine("  - search_company_filings      Search company filings");
    Console.WriteLine("  - get_latest_filings          Get latest filings");
    Console.WriteLine("  - get_filing_details          Get filing details");
    Console.WriteLine("  - preview_filing_sections     Preview filing sections");
    Console.WriteLine("  - get_filing_sections         Get specific filing sections");
    Console.WriteLine("  - And 8 more filing-related tools...");
    Console.WriteLine();
    Console.WriteLine("MCP Server Prompts:");
    Console.WriteLine("  - comprehensive-company-analysis");
    Console.WriteLine("  - business-value-assessment");
    Console.WriteLine("  - compare-peers");
    Console.WriteLine("  - query-filing");
    Console.WriteLine("  - track-changes");
    Console.WriteLine("  - discover-filings");
}

static async Task StartMcpServer(string serverPath)
{
    Console.WriteLine("Starting MCP server in interactive mode...");
    Console.WriteLine("The server will communicate via stdio (standard input/output)");
    Console.WriteLine();
    Console.WriteLine("Press Ctrl+C to stop the server.");
    Console.WriteLine("═══════════════════════════════════════════════");
    Console.WriteLine();

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = serverPath,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            UseShellExecute = false,
            CreateNoWindow = false,
            Environment =
            {
                ["EDGAR_APP_NAME"] = "Moedim.Edgar.Mcp.Client",
                ["EDGAR_APP_VERSION"] = "1.0.0",
                ["EDGAR_EMAIL"] = Environment.GetEnvironmentVariable("EDGAR_EMAIL") ?? "test@example.com",
                ["EDGAR_REQUEST_DELAY_MS"] = "100",
                ["EDGAR_MAX_RETRY_COUNT"] = "3"
            }
        }
    };

    process.Start();
    await process.WaitForExitAsync();
}

static async Task TestStdioConnection(string serverPath)
{
    Console.WriteLine("Testing stdio connection to MCP server...");
    Console.WriteLine();

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = serverPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["EDGAR_APP_NAME"] = "Moedim.Edgar.Mcp.Client",
                ["EDGAR_APP_VERSION"] = "1.0.0",
                ["EDGAR_EMAIL"] = Environment.GetEnvironmentVariable("EDGAR_EMAIL") ?? "test@example.com",
                ["EDGAR_REQUEST_DELAY_MS"] = "100",
                ["EDGAR_MAX_RETRY_COUNT"] = "3"
            }
        }
    };

    process.ErrorDataReceived += (sender, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Console.WriteLine($"[SERVER ERROR] {e.Data}");
        }
    };

    process.Start();
    process.BeginErrorReadLine();

    Console.WriteLine("✓ Server process started");
    Console.WriteLine();
    Console.WriteLine("Sending initialize request...");

    // Send MCP initialize request
    var initRequest = JsonSerializer.Serialize(new
    {
        jsonrpc = "2.0",
        id = 1,
        method = "initialize",
        @params = new
        {
            protocolVersion = "2024-11-05",
            clientInfo = new
            {
                name = "Moedim.Edgar.Mcp.Client",
                version = "1.0.0"
            },
            capabilities = new { }
        }
    });

    await process.StandardInput.WriteLineAsync(initRequest);
    await process.StandardInput.FlushAsync();

    Console.WriteLine($"Sent: {initRequest}");
    Console.WriteLine();

    // Read response
    var response = await ReadJsonRpcResponseAsync(process.StandardOutput);
    if (response != null)
    {
        Console.WriteLine($"Received: {response}");
        Console.WriteLine();
        Console.WriteLine("✓ Successfully connected to MCP server!");
    }
    else
    {
        Console.WriteLine("✗ Failed to receive response from server");
    }

    // Cleanup
    process.Kill();
    await process.WaitForExitAsync();
}

static async Task RunSampleInteraction(string serverPath, string ticker)
{
    Console.WriteLine($"Running sample MCP interaction for ticker: {ticker}");
    Console.WriteLine();

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = serverPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            Environment =
            {
                ["EDGAR_APP_NAME"] = "Moedim.Edgar.Mcp.Client",
                ["EDGAR_APP_VERSION"] = "1.0.0",
                ["EDGAR_EMAIL"] = Environment.GetEnvironmentVariable("EDGAR_EMAIL") ?? "test@example.com",
                ["EDGAR_REQUEST_DELAY_MS"] = "100",
                ["EDGAR_MAX_RETRY_COUNT"] = "3"
            }
        }
    };

    var errorOutput = new StringBuilder();
    process.ErrorDataReceived += (sender, e) =>
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            errorOutput.AppendLine(e.Data);
        }
    };

    process.Start();
    process.BeginErrorReadLine();

    try
    {
        // Initialize
        Console.WriteLine("1. Initializing MCP connection...");
        var initRequest = new
        {
            jsonrpc = "2.0",
            id = 1,
            method = "initialize",
            @params = new
            {
                protocolVersion = "2024-11-05",
                clientInfo = new { name = "TestClient", version = "1.0.0" },
                capabilities = new { }
            }
        };

        await SendJsonRpcRequestAsync(process.StandardInput, initRequest);
        var initResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);
        Console.WriteLine($"   ✓ Initialized");
        Console.WriteLine();

        // List tools
        Console.WriteLine("2. Listing available tools...");
        var listToolsRequest = new
        {
            jsonrpc = "2.0",
            id = 2,
            method = "tools/list",
            @params = new { }
        };

        await SendJsonRpcRequestAsync(process.StandardInput, listToolsRequest);
        var toolsResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);

        if (toolsResponse != null)
        {
            var toolsJson = JsonSerializer.Deserialize<JsonElement>(toolsResponse);
            if (toolsJson.TryGetProperty("result", out var result) &&
                result.TryGetProperty("tools", out var tools))
            {
                var toolArray = tools.EnumerateArray().ToList();
                Console.WriteLine($"   ✓ Found {toolArray.Count} tools");
                Console.WriteLine();
                Console.WriteLine("   Available tools:");
                foreach (var tool in toolArray.Take(5))
                {
                    if (tool.TryGetProperty("name", out var name))
                    {
                        Console.WriteLine($"     - {name.GetString()}");
                    }
                }
                if (toolArray.Count > 5)
                {
                    Console.WriteLine($"     ... and {toolArray.Count - 5} more");
                }
            }
        }
        Console.WriteLine();

        // Call get_cik_from_symbol
        Console.WriteLine($"3. Getting CIK for {ticker}...");
        var getCikRequest = new
        {
            jsonrpc = "2.0",
            id = 3,
            method = "tools/call",
            @params = new
            {
                name = "get_cik_from_symbol",
                arguments = new { symbol = ticker }
            }
        };

        await SendJsonRpcRequestAsync(process.StandardInput, getCikRequest);
        var cikResponse = await ReadJsonRpcResponseAsync(process.StandardOutput);

        if (cikResponse != null)
        {
            var cikJson = JsonSerializer.Deserialize<JsonElement>(cikResponse);
            if (cikJson.TryGetProperty("result", out var result) &&
                result.TryGetProperty("content", out var content))
            {
                var contentArray = content.EnumerateArray().FirstOrDefault();
                if (contentArray.TryGetProperty("text", out var text))
                {
                    Console.WriteLine($"   ✓ {text.GetString()}");
                }
            }
        }
        Console.WriteLine();

        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine("  Sample interaction completed successfully!");
        Console.WriteLine("═══════════════════════════════════════════════");
    }
    finally
    {
        // Cleanup
        process.Kill();
        await process.WaitForExitAsync();

        if (errorOutput.Length > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Server logs:");
            Console.WriteLine(errorOutput.ToString());
        }
    }
}

static async Task SendJsonRpcRequestAsync(StreamWriter writer, object request)
{
    var json = JsonSerializer.Serialize(request);
    await writer.WriteLineAsync(json);
    await writer.FlushAsync();
}

static async Task<string?> ReadJsonRpcResponseAsync(StreamReader reader)
{
    var line = await reader.ReadLineAsync();
    return line;
}
