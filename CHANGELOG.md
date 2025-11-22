# Changelog

All notable changes to Moedim.Edgar will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0]

### Added

- **Filing Document Retrieval** - New `GetFilingDocument` method to download full SEC filing content in markdown or HTML format
- **Filing Section Preview** - New `PreviewFilingSections` method to preview available sections in a filing without downloading full content
- **Filing Section Retrieval** - New `GetFilingSections` method to retrieve specific sections by anchor IDs
- **Caching System** - Added `LocalFileCache` implementation for caching SEC EDGAR data to improve performance and reduce API calls
- **Filing Processor** - New `FilingProcessor` for processing HTML content into structured sections
- **New Models** - Added `FilingSectionsRequest`, `FilingSectionsResult`, `HtmlSlice`, and `SectionPreview` models for structured data handling
- **MCP Server Enhancements** - Added filing details tools and prompts to the MCP server (17 tools and 6 prompts total)
- **MCP Client Sample** - New sample project demonstrating MCP client implementation

### Changed

- Enhanced `FilingDetailsService` to utilize caching and markdown conversion
- Updated dependency injection to register caching service
- Improved sample applications with comprehensive examples
- Updated README with expanded documentation and usage examples

### Fixed

- Performance improvements through intelligent caching strategy
- Better error handling in filing retrieval operations

## [1.0.0] - 2025-11-18

### Added

- Initial release of Moedim.Edgar
- SEC EDGAR API client with IHttpClientFactory support
- Company Facts service for retrieving financial data
- Company Concept service for specific concept queries
- Dependency injection extensions for easy integration
- Type-safe models for EDGAR data structures
- Support for .NET 8.0
- Comprehensive XML documentation on public APIs
- Modern async/await patterns throughout

### Features

- **Modern HTTP Client** - Uses IHttpClientFactory for efficient HTTP communication
- **Dependency Injection** - First-class DI support with extension methods
- **Type-Safe Models** - Strongly-typed models for EDGAR data
- **Async/Await** - Fully asynchronous API
- **Configurable** - Flexible options pattern for configuration
- **Well-Documented** - XML documentation on all public APIs

[1.0.0]: https://github.com/kdcllc/Moedim.Edgar/releases/tag/v1.0.0
