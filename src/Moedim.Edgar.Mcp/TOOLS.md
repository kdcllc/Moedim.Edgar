# Moedim.Edgar.Mcp Tools & Prompts Documentation

This MCP server exposes 17 comprehensive tools and 6 guided prompts for accessing and analyzing SEC EDGAR data through the Model Context Protocol.

## Table of Contents

- [Tools (17 total)](#tools)
  - [Company Data Tools (4)](#-company-data-tools-4-tools)
  - [Filing Search Tools (4)](#-filing-search-tools-4-tools)
  - [Filing Details Tools (9)](#-filing-details-tools-9-tools)
- [Prompts (6 total)](#prompts)
  - [Analysis Workflows](#analysis-workflows)
- [Configuration](#-configuration)
- [Usage Examples](#-usage-examples)
- [Resources](#-related-resources)

---

# Tools

## 📊 Company Data Tools (4 tools)

Tools for retrieving company financial facts and concepts from SEC EDGAR.

### 1. `get_cik_from_symbol`

Convert a stock ticker symbol to a CIK number.

**Parameters:**

- `symbol` (string, required): Stock ticker symbol (e.g., "AAPL", "MSFT", "TSLA")

**Returns:** CIK number as a string

**Example:**

```json
{
  "symbol": "AAPL"
}
```

---

### 2. `get_company_facts`

Get all financial facts for a company by CIK.

**Parameters:**

- `cik` (string, required): Company CIK number

**Returns:** Complete company facts data including all financial metrics across all reporting periods

**Example:**

```json
{
  "cik": "0000320193"
}
```

---

### 3. `get_company_concept`

Get historical data for a specific financial concept (e.g., revenue, assets).

**Parameters:**

- `cik` (string, required): Company CIK number
- `taxonomy` (string, required): Taxonomy (usually "us-gaap", "ifrs-full", or "dei")
- `tag` (string, required): Concept tag (e.g., "Revenues", "Assets", "NetIncomeLoss")

**Returns:** Historical data for the specified concept including values across different reporting periods and forms

**Example:**

```json
{
  "cik": "0000320193",
  "taxonomy": "us-gaap",
  "tag": "Revenues"
}
```

---

### 4. `list_common_concepts`

List commonly used financial concepts for reference.

**Parameters:** None

**Returns:** List of common US-GAAP concepts with descriptions

**Categories:**

- Income Statement concepts (Revenues, NetIncomeLoss, etc.)
- Balance Sheet concepts (Assets, Liabilities, Equity)
- Cash Flow concepts (Operating, Investing, Financing activities)
- Per Share metrics (EPS, Book Value)

---

## 🔍 Filing Search Tools (4 tools)

Tools for searching and discovering SEC filings.

### 5. `search_company_filings`

Search for company filings with flexible filtering options.

**Parameters:**

- `symbol` (string, optional): Stock ticker symbol
- `cik` (string, optional): Company CIK number
- `filingType` (string, optional): Filing form type (e.g., "10-K", "10-Q", "8-K")
- `startDate` (string, optional): Start date for date range (YYYY-MM-DD)
- `endDate` (string, optional): End date for date range (YYYY-MM-DD)

**Note:** Either `symbol` or `cik` must be provided

**Returns:** Search results with filing details and pagination support

**Example:**

```json
{
  "symbol": "AAPL",
  "filingType": "10-K",
  "startDate": "2023-01-01",
  "endDate": "2023-12-31"
}
```

---

### 6. `get_latest_filings`

Get the most recent filings for a company.

**Parameters:**

- `symbol` (string, optional): Stock ticker symbol
- `cik` (string, optional): Company CIK number
- `count` (integer, optional): Number of filings to retrieve (default: 20, max: 100)

**Note:** Either `symbol` or `cik` must be provided

**Returns:** List of latest filings with details

**Example:**

```json
{
  "symbol": "MSFT",
  "count": 10
}
```

---

### 7. `get_next_filings_page`

Get the next page of search results from a previous search.

**Parameters:**

- `nextPageUrl` (string, required): URL from a previous search result's `NextPageUrl` field

**Returns:** Next page of search results

**Example:**

```json
{
  "nextPageUrl": "https://efts.sec.gov/LATEST/search-index?page=2&..."
}
```

---

### 8. `list_common_form_types`

List commonly used SEC form types with descriptions.

**Parameters:** None

**Returns:** List of common SEC forms with descriptions

**Categories:**

- Annual/Quarterly Reports (10-K, 10-Q)
- Current Events (8-K)
- Proxy Statements (DEF 14A)
- Registration Statements (S-1, S-3)
- And more...

---

## 📄 Filing Details Tools (9 tools)

Tools for accessing detailed filing information, documents, and intelligent section extraction.

### 9. `get_filing_details`

Get comprehensive details about a specific filing.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number (format: 0000000000-00-000000)

**Returns:** Complete filing details including documents, data files, and metadata

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077"
}
```

---

### 10. `get_cik_from_filing`

Extract the company CIK from a filing accession number.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number

**Returns:** Company CIK number

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077"
}
```

---

### 11. `get_document_format_files`

Get all document format files (HTML, XBRL, etc.) from a filing.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number

**Returns:** List of document files with URLs, sizes, and descriptions

**File Types:**

- Primary documents (10-K, 10-Q, 8-K HTML/XML)
- XBRL instance documents
- Exhibits
- Supporting documents

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077"
}
```

---

### 12. `get_data_files`

Get structured data files (XML, JSON, XBRL) from a filing.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number

**Returns:** List of data files for programmatic access

**Data File Types:**

- XBRL data files (.xml)
- Financial statements in iXBRL
- Company data in JSON format
- Schema and taxonomy files

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077"
}
```

---

### 14. `get_filing_document`

Download the complete content of a SEC filing document in markdown or HTML format.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number (format: 0000000000-00-000000)
- `tickerOrCik` (string, optional): Company ticker symbol or CIK to help locate the filing
- `format` (string, optional): Output format - 'markdown' (default, clean text) or 'html' (raw HTML)

**Returns:** Complete filing content in the requested format

**Warning:** Filings can be very large (10-50MB for 10-K forms). For targeted analysis, use `preview_filing_sections` and `get_filing_sections` instead.

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077",
  "tickerOrCik": "AAPL",
  "format": "markdown"
}
```

---

### 15. `preview_filing_sections`

Preview all available sections in a SEC filing WITHOUT retrieving full content. Returns section IDs, titles, and brief snippets.

**CRITICAL:** This does NOT return full section content - only headers and previews. Use this FIRST to discover what sections are available, then call `get_filing_sections` with the anchor IDs to retrieve actual full content.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number
- `tickerOrCik` (string, optional): Company ticker symbol or CIK

**Returns:** Table of contents with section IDs, titles, and brief snippets

**Workflow:**

1. Call `preview_filing_sections` to see available sections
2. Identify which anchor IDs contain information you need
3. Call `get_filing_sections` with those anchor IDs to get full content

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077",
  "tickerOrCik": "AAPL"
}
```

**Example Response:**

```json
{
  "TotalSections": 5,
  "Sections": [
    {
      "AnchorId": "part_i",
      "Title": "PART I",
      "Preview": "Item 1. Business Apple Inc. designs..."
    },
    {
      "AnchorId": "item_1a_risk_factors",
      "Title": "ITEM 1A. RISK FACTORS",
      "Preview": "Investing in our securities involves risk..."
    }
  ]
}
```

---

### 16. `get_filing_sections`

Retrieve FULL CONTENT of specific filing sections by anchor IDs in clean markdown format by default.

**IMPORTANT:** You must call `preview_filing_sections` first to get the anchor IDs. This tool retrieves actual section content based on the IDs you provide.

**Parameters:**

- `accessionNumber` (string, required): Filing accession number
- `anchorIds` (array of strings, required): List of section IDs to retrieve (from `preview_filing_sections`)
- `tickerOrCik` (string, optional): Company ticker symbol or CIK
- `merge` (boolean, optional): If true, combines all sections into single content block. If false, returns sections separately.
- `format` (string, optional): Output format - 'markdown' (default) or 'html'

**Returns:** Full content of the specified sections in the requested format

**Use Cases:**

- Extract specific sections for analysis (e.g., Risk Factors, MD&A)
- Retrieve multiple related sections
- Get merged content from multiple sections
- Access raw HTML sections if needed

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077",
  "anchorIds": ["item_1a_risk_factors", "item_7_managements_discussion_analysis"],
  "merge": true,
  "format": "markdown"
}
```

---

### 17. `get_primary_document_url`

Get the URL for the primary document (main filing content).

**Parameters:**

- `accessionNumber` (string, required): Filing accession number

**Returns:** URL to the primary document

**Example:**

```json
{
  "accessionNumber": "0000320193-23-000077"
}
```

---

## 🔧 Configuration

The MCP server requires SEC EDGAR configuration via environment variables:

```bash
# Required
EDGAR_APP_NAME="Your App Name"
EDGAR_APP_VERSION="1.0.0"
EDGAR_EMAIL="your.email@example.com"

# Optional
EDGAR_REQUEST_DELAY_MS=100       # Delay between requests
EDGAR_MAX_RETRY_COUNT=3           # Max retry attempts
```

## 🚀 Usage Examples

### Example 1: Analyze Company Financials

```
1. get_cik_from_symbol(symbol: "AAPL")
2. get_company_facts(cik: "0000320193")
3. get_company_concept(cik: "0000320193", taxonomy: "us-gaap", tag: "Revenues")
```

### Example 2: Find Recent Quarterly Reports

```
1. search_company_filings(symbol: "MSFT", filingType: "10-Q")
2. get_filing_details(accessionNumber: "...")
3. get_data_files(accessionNumber: "...")
```

### Example 3: Track Latest Company Activity

```
1. get_latest_filings(symbol: "TSLA", count: 20)
2. Filter for 8-K forms to see current events
3. get_filing_details for interesting filings
```

### Example 4: Extract Specific Filing Sections (NEW)

```
1. preview_filing_sections(accessionNumber: "0000320193-23-000077")
2. Review available sections and their anchor IDs
3. get_filing_sections(accessionNumber: "0000320193-23-000077",
                       anchorIds: ["item_1a_risk_factors", "item_7_managements_discussion_analysis"],
                       merge: true)
4. Analyze the extracted sections
```

### Example 5: Download Complete Filing as Markdown (NEW)

```
1. get_filing_document(accessionNumber: "0000320193-23-000077", format: "markdown")
2. Process clean markdown text with LLM
```

## 📋 Common Financial Concepts

### Income Statement

- `Revenues` - Total revenue
- `NetIncomeLoss` - Net income/loss
- `OperatingIncomeLoss` - Operating income
- `GrossProfit` - Gross profit
- `CostOfRevenue` - Cost of goods sold

### Balance Sheet

- `Assets` - Total assets
- `Liabilities` - Total liabilities
- `StockholdersEquity` - Shareholders equity
- `AssetsCurrent` - Current assets
- `LiabilitiesCurrent` - Current liabilities

### Cash Flow

- `NetCashProvidedByUsedInOperatingActivities` - Operating cash flow
- `NetCashProvidedByUsedInInvestingActivities` - Investing cash flow
- `NetCashProvidedByUsedInFinancingActivities` - Financing cash flow

### Per Share Metrics

- `EarningsPerShareBasic` - Basic EPS
- `EarningsPerShareDiluted` - Diluted EPS
- `CommonStockSharesOutstanding` - Shares outstanding

## 📝 Common SEC Forms

- **10-K**: Annual report with comprehensive financial information
- **10-Q**: Quarterly report with unaudited financial statements
- **8-K**: Current events report (acquisitions, executive changes, etc.)
- **DEF 14A**: Proxy statement for shareholder meetings
- **S-1**: Initial registration for new securities
- **S-3**: Simplified registration for seasoned issuers
- **4**: Insider trading report
- **13F**: Institutional investment manager holdings
- **144**: Notice of proposed sale of securities

## 🔗 Related Resources

- [SEC EDGAR API Documentation](https://www.sec.gov/edgar/sec-api-documentation)
- [XBRL US GAAP Taxonomy](https://xbrl.us/xbrl-taxonomy/2024-us-gaap/)
- [Model Context Protocol](https://modelcontextprotocol.io/)

---

# Prompts

The MCP server provides 6 pre-built prompts that guide LLMs through structured financial analysis workflows. These prompts automatically chain together the appropriate tools to accomplish complex analysis tasks.

## Analysis Workflows

### 1. `comprehensive-company-analysis`

**Purpose:** Complete company analysis covering financial performance, risks, strategy, and operations.

**Parameters:**
- `ticker` (string, required): Company ticker symbol
- `focusAreas` (string, optional): Areas to focus on (default: "financial performance, risks, strategy, operations")
- `timePeriod` (string, optional): Time period to analyze (default: "latest")

**Workflow:**
1. Retrieves company facts and basic information
2. Gathers relevant filings (10-K, 10-Q, 8-K)
3. Previews and extracts targeted sections
4. Analyzes trends across multiple periods
5. Synthesizes findings into structured report

**Output:** Structured report with summary, findings table, and actionable implications with citations.

**Example Usage:**
```
Use comprehensive-company-analysis prompt for AAPL focusing on "growth strategy, competitive risks"
```

---

### 2. `business-value-assessment`

**Purpose:** Four-lens viability assessment (Growth, Efficiency, Resilience, Technology) with GREEN/YELLOW/RED scorecard.

**Parameters:**
- `ticker` (string, required): Company ticker symbol
- `formType` (string, optional): Filing type to analyze (default: "10-Q")
- `compareToPeriodsAgo` (string, optional): Number of periods to compare (default: "4")

**Workflow:**
1. Gathers filings for trend comparison
2. Extracts MD&A, financial statements, risk factors
3. Builds metrics dashboard
4. Assesses viability across four lenses
5. Generates color-coded scorecard

**Four Lenses:**
- **Growth**: Revenue trends, diversification, customer base
- **Efficiency**: Margins, cost structure, working capital
- **Resilience**: Liquidity, leverage, free cash flow
- **Technology**: R&D investment, innovation, competitive moat

**Output:** Summary verdict, metrics table with trend indicators, scorecard with justifications, and flagged issues.

**Example Usage:**
```
Use business-value-assessment prompt for MSFT with 10-K filings comparing 3 periods
```

---

### 3. `compare-peers`

**Purpose:** Multi-company comparison with differentiator matrix.

**Parameters:**
- `tickers` (string, required): Comma-separated list of ticker symbols
- `comparisonFocus` (string, optional): Focus areas for comparison (default: "all")
- `formType` (string, optional): Filing type to use (default: "10-K")

**Workflow:**
1. Retrieves company facts for all tickers
2. Gathers aligned filings
3. Extracts parallel sections (business, risks, financials, MD&A)
4. Builds comparison matrix
5. Identifies differentiators

**Output:** Summary of leaders/laggards, comparison table with key metrics, and insights on differentiators.

**Example Usage:**
```
Use compare-peers prompt for "AAPL,MSFT,GOOGL" focusing on "innovation and profitability"
```

---

### 4. `query-filing`

**Purpose:** Extract specific information from a filing using intelligent section matching.

**Parameters:**
- `ticker` (string, required): Company ticker symbol
- `accessionNumber` (string, required): Filing accession number
- `query` (string, required): What information to extract

**Workflow:**
1. Previews available sections
2. Matches query to relevant sections intelligently
3. Retrieves matched section content
4. Scans for specific query details

**Section Matching:**
- Financials → Item 8/Financial Statements
- Risks → Item 1A/Risk Factors
- Business → Item 1
- Performance → MD&A/Item 7
- Legal → Item 3

**Output:** Direct answer with supporting quotes and citations.

**Example Usage:**
```
Use query-filing prompt for TSLA filing 0001318605-24-000008 asking "What are the main supply chain risks?"
```

---

### 5. `track-changes`

**Purpose:** Monitor how specific disclosure sections evolve across multiple periods with implication scoring.

**Parameters:**
- `ticker` (string, required): Company ticker symbol
- `sectionFocus` (string, required): Section to track (e.g., "Risk Factors", "MD&A")
- `formType` (string, optional): Filing type (default: "10-K")
- `periods` (string, optional): Number of periods to track (default: "3")

**Workflow:**
1. Gathers historical filings
2. Extracts target sections from each period
3. Performs chronological comparison
4. Scores implications (+1/0/-1)
5. Identifies trends

**Output:** Timeline table with changes, implications, and scores, plus summary of evolution.

**Example Usage:**
```
Use track-changes prompt for NVDA tracking "Risk Factors" in 10-K over 4 periods
```

---

### 6. `discover-filings`

**Purpose:** Discover relevant filings based on search criteria using strategy-based selection.

**Parameters:**
- `searchGoal` (string, required): What you're trying to find
- `tickers` (string, optional): Target companies (if known)

**Workflow:**
1. Defines search criteria (companies, forms, dates, limits)
2. Selects appropriate search tool(s)
3. Executes search strategy
4. Filters by relevance
5. Prepares for content extraction

**Available Strategies:**
- Specific company recent filings → `search_company_filings`
- Latest across market → `get_latest_filings`

**Output:** Search strategy execution with filtered results and next steps.

**Example Usage:**
```
Use discover-filings prompt to find "recent cybersecurity incident disclosures in tech sector"
```

---

## Using Prompts

Prompts are invoked through your MCP client (e.g., Claude Desktop, VS Code Copilot). The LLM will:

1. Understand the analysis goal from your request
2. Select the appropriate prompt template
3. Execute the structured workflow
4. Chain together the necessary tools
5. Present findings in the specified format

**Benefits:**
- Consistent analysis methodology
- Automatic tool chaining
- Structured outputs
- Best practices built-in
- Reduces need for prompt engineering

---

## 🔗 Related Resources

- [SEC EDGAR API Documentation](https://www.sec.gov/edgar/sec-api-documentation)
- [XBRL US GAAP Taxonomy](https://xbrl.us/xbrl-taxonomy/2024-us-gaap/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
