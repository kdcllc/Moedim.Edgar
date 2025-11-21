# Moedim.Edgar.Mcp Tools Documentation

This MCP server exposes 13 comprehensive tools for accessing SEC EDGAR data through the Model Context Protocol.

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

## 📄 Filing Details Tools (5 tools)

Tools for accessing detailed filing information and documents.

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

### 13. `get_primary_document_url`
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
