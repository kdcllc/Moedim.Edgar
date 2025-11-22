using ModelContextProtocol.Server;

namespace Moedim.Edgar.Mcp.Prompts;

/// <summary>
/// MCP prompts for guiding LLM interactions with SEC Edgar tools.
/// Provides structured workflows for common financial analysis tasks.
/// </summary>
[McpServerPromptType]
public class EdgarPrompts
{
    /// <summary>
    /// Guides comprehensive company analysis using SEC filings.
    /// Focus: Iterative discovery, targeted sections, multi-period trends.
    /// </summary>
    [McpServerPrompt(Name = "comprehensive-company-analysis")]
    public string ComprehensiveCompanyAnalysisPrompt(
        string ticker,
        string? focusAreas = "financial performance, risks, strategy, operations",
        string? timePeriod = "latest")
    {
        return $"""
        You are a financial analyst analyzing {ticker} via SEC filings. Focus: {focusAreas}. Period: {timePeriod}.

        **Core Question**: What are key trends in performance, risks, and strategy?

        **Phased Workflow** (Reason then act with tools):

        1. **Overview**: Call get_company_facts({ticker}) to get company basics and recent financial facts.

        2. **Gather Filings**:
           - search_company_filings("{ticker}", "10-K", 1) for annual report.
           - search_company_filings("{ticker}", "10-Q", 2) for recent quarters.
           - search_company_filings("{ticker}", "8-K", 5) for recent material events.
           Record accession numbers.

        3. **Target Sections** (Per filing; preview first):
           - Call preview_filing_sections(ticker, accessionNumber) to list available sections.
           - Call get_filing_sections(ticker, accessionNumber, relevant anchors) for:
             - Risks: "Risk Factors"/Item 1A
             - Financials: "Item 8"/Financial Statements
             - Strategy: "Item 1"/"Item 7" (MD&A)
             - Operations: "Results of Operations"
           - Validate: Cross-check MD&A narrative vs. financial numbers (flag discrepancies).

        4. **Analyze Trends**: Compare periods for changes (e.g., risk escalation, metric shifts). Weight recent data.

        5. **Synthesize**: Identify strengths, risks, opportunities, and overall health verdict.

        **Output**: Structured report with citations (accession + section).
        - **Summary**: 3 bullets on trajectory.
        - **Key Findings Table**:
          | Area | Trend | Evidence (Cite) | Flag |
          |------|--------|-----------------|------|
          | ... | ... | ... | ... |
        - **Implications**: 2-3 actionable insights.

        Begin analysis.
        """;
    }

    /// <summary>
    /// Assesses business viability via 4 lenses: Growth, Efficiency, Resilience, Tech.
    /// Outputs GREEN/YELLOW/RED scorecard with integrated metrics dashboard.
    /// </summary>
    [McpServerPrompt(Name = "business-value-assessment")]
    public string BusinessValueAssessmentPrompt(
        string ticker,
        string? formType = "10-Q",
        string? compareToPeriodsAgo = "4")
    {
        return $"""
        Assess {ticker}'s viability using {formType} filings. Question: Sustainable growth, efficiency, security, tech readiness?

        **Lenses**: GREEN (>strong), YELLOW (adequate/mixed), RED (<weak). Justify with 1-2 sentences each.

        **Phase 1: Data**:
        - search_company_filings("{ticker}", "{formType}", 1 + {compareToPeriodsAgo}) for trend comparison.
        - search_company_filings("{ticker}", "8-K", 3) for recent material events.
        - Per accession: preview_filing_sections → get_filing_sections for MD&A (Item 2/7), Statements (Balance/Income/Cash), Segments, Risks (Item 1A), Footnotes.

        **Phase 2: Analyze** (Build dashboard; validate narrative vs. numbers; flag anomalies):
        - **Growth**: Revenue YoY/QoQ, revenue mix (segments/geography), customer concentration. GREEN: >10% growth, diversified.
        - **Efficiency**: Margins (gross/operating), costs (COGS/SG&A), working capital, CapEx. GREEN: Expanding margins, scalable operations.
        - **Resilience**: Liquidity (current ratio), leverage (D/E, interest coverage), Free Cash Flow. Stress test: Can service debt? Runway in quarters? GREEN: Positive FCF, low leverage.
        - **Tech**: R&D as % of revenue, innovation (new products/patents), competitive moat (data/network effects), risks (cybersecurity/obsolescence). GREEN: R&D driving growth, modern tech stack.

        **Phase 3: Output**:
        - **Summary**: 2 sentences on current state + overall verdict (PASS: Mostly GREEN; WATCH: Mixed; FAIL: 2+ RED).
        - **Metrics Table** (Current vs. Prior; Trend ↑/↓/→; Flag 🟢/🟡/🔴):
          | Metric | Current | Prior | Trend | Flag |
          |--------|---------|-------|--------|------|
          | Rev Growth | X% | Y% | ↑ | 🟢 |
          | Op Margin | X% | Y% | ↓ | 🔴 |
          | FCF | $Xm | $Ym | → | 🟡 |
          | D/E | Xx | Yy | ↑ | 🔴 |
          | R&D % | X% | Y% | ↑ | 🟢 |
        - **Scorecard**:
          | Lens | Verdict | Justification |
          |------|---------|---------------|
          | Growth | ... | ... |
          | Efficiency | ... | ... |
          | Resilience | ... | ... |
          | Technology | ... | ... |
        - **Issues**: 3 bullets (risks, anomalies, tone shifts). Cite sections/accessions.

        Principles: Validate MD&A vs. financial tables; flag inconsistencies; contextualize within industry norms.

        Start assessment.
        """;
    }

    /// <summary>
    /// Compares peer companies in the same industry.
    /// Performs parallel extraction and creates differentiator matrix.
    /// </summary>
    [McpServerPrompt(Name = "compare-peers")]
    public string ComparePeersPrompt(
        string tickers,
        string? comparisonFocus = "all",
        string? formType = "10-K")
    {
        var tickerList = tickers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var tickerBullets = string.Join("\n", tickerList.Select(t => $"   - {t}"));

        return $"""
        Compare peers: {tickerBullets}. Focus: {comparisonFocus}. Forms: {formType}.

        **Workflow**:
        1. **Basics**: For each ticker, get_company_facts(ticker). Note business models and industry.
        2. **Filings**: search_company_filings(ticker, "{formType}", 1) for each. Align periods by filing date.
        3. **Extract**: Per filing, preview_filing_sections → get_filing_sections for:
           - Business description (Item 1)
           - Risk Factors (Item 1A)
           - Financial Statements (Item 8)
           - MD&A (Item 7)
        4. **Build Comparison Matrix** (validate data is comparable):
           - **Metrics**: Revenue, margins, Free Cash Flow
           - **Strategy**: Priorities, key investments
           - **Risks**: Unique vs. shared risks
           - **Market Position**: Competitive moats, financial health
        5. **Identify Differentiators**: Which company is strongest in growth? Lowest risk? Final verdict per company.

        **Output**: Tables + highlights.
        - **Summary**: 3 bullets on leaders/laggards.
        - **Comparison Table**:
          | Metric/Area | {string.Join(" | ", tickerList)} |
          |-------------|----------------------------------|
          | Rev Growth | X% \| Y% \| ... |
          | Op Margin | X% \| Y% \| ... |
          | FCF | $Xm \| $Ym \| ... |
          | Key Risks | ... \| ... \| ... |
        - **Insights**: 2-3 key differentiators per company. Cite accessions/sections.

        Begin comparison.
        """;
    }

    /// <summary>
    /// Queries specific information from a filing.
    /// Uses intelligent section matching with context extraction.
    /// </summary>
    [McpServerPrompt(Name = "query-filing")]
    public string QueryFilingPrompt(
        string ticker,
        string accessionNumber,
        string query)
    {
        return $"""
        Extract from {ticker} filing {accessionNumber}: {query}.

        **Steps**:
        1. preview_filing_sections("{ticker}", "{accessionNumber}"). Review section titles and snippets.
        2. Match query to relevant sections:
           - Financials → Item 8/Financial Statements
           - Risks → Item 1A/Risk Factors
           - Business description → Item 1
           - Performance commentary → MD&A/Item 7
           - Legal matters → Item 3
        3. get_filing_sections("{ticker}", "{accessionNumber}", matched anchor IDs; merge=true). Retrieve content.
        4. Scan content for query details (numbers, dates, specific context). Validate relevance.

        **Output**: Direct answer + 2-3 supporting quotes. Cite section/accession.

        Extract now.
        """;
    }

    /// <summary>
    /// Tracks how specific disclosure sections change over time.
    /// Performs chronological diff with implication scoring.
    /// </summary>
    [McpServerPrompt(Name = "track-changes")]
    public string TrackChangesPrompt(
        string ticker,
        string sectionFocus,
        string? formType = "10-K",
        string? periods = "3")
    {
        return $"""
        Track {sectionFocus} evolution for {ticker} in {periods} {formType} filings.

        **Steps**:
        1. search_company_filings("{ticker}", "{formType}", {periods}). List filing dates and accession numbers.
        2. For each filing: preview_filing_sections → get_filing_sections for sections matching {sectionFocus} (by title/ID).
        3. Compare chronologically: Identify new content, removed content, modified sections (e.g., added risks, tone shifts).
        4. Analyze trends: Is disclosure escalating or de-escalating? Positive or negative implications? Score each change: +1 (positive) / 0 (neutral) / -1 (negative).
        5. Assess implications: What do changes signal about company direction? What questions do they raise?

        **Output**: Timeline table.
        | Period | Key Changes | Implications (Score) | Cite |
        |--------|-------------|----------------------|------|
        | Latest | ... | ... | ... |
        | ... | ... | ... | ... |

        Plus 3 bullets summarizing overall evolution.

        Start tracking.
        """;
    }

    /// <summary>
    /// Discovers relevant filings based on search criteria.
    /// Uses criteria-based strategy selection.
    /// </summary>
    [McpServerPrompt(Name = "discover-filings")]
    public string DiscoverFilingsPrompt(
        string searchGoal,
        string? tickers = null)
    {
        var tickerGuidance = string.IsNullOrEmpty(tickers)
            ? "Identify tickers via get_company_facts if needed."
            : $"Target: {tickers}";

        return $"""
        Discover filings for: {searchGoal}. {tickerGuidance}

        **Available Tools**:
        - search_company_filings: Search by ticker, form type, count
        - get_latest_filings: Get recent filings across all companies

        **Approach**:
        1. Define criteria: Which companies? Which forms (10-K/10-Q/8-K)? Time period? Count limit?
        2. Select and call appropriate tool:
           - For specific company recent filings → search_company_filings(ticker, formType, count)
           - For latest filings across market → get_latest_filings(count)
        3. Filter results by relevance (date, form type); note accession numbers.
        4. Next step: Use preview_filing_sections and get_filing_sections on selected filings to extract content matching the search goal.

        Execute search strategy now.
        """;
    }
}
