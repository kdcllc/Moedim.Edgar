namespace Moedim.Edgar.Options;

/// <summary>
/// Configuration options for SEC EDGAR client
/// </summary>
public class SecEdgarOptions
{
    /// <summary>
    /// User agent information required by SEC
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Base URL for SEC EDGAR API (default: https://data.sec.gov/api/xbrl)
    /// </summary>
    public string BaseApiUrl { get; set; } = "https://data.sec.gov/api/xbrl";

    /// <summary>
    /// Delay between requests (default: 250ms as per SEC guidelines)
    /// </summary>
    public TimeSpan RequestDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Delay after throttling or errors (default: 2 seconds)
    /// </summary>
    public TimeSpan TimeoutDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Maximum number of retry attempts (default: 10)
    /// </summary>
    public int MaxRetryCount { get; set; } = 10;

    /// <summary>
    /// Application name for user agent
    /// </summary>
    public string? AppName { get; set; }

    /// <summary>
    /// Application version for user agent
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// Contact email for user agent
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Validates the options configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AppName))
        {
            throw new InvalidOperationException("AppName is required for SEC user agent identification.");
        }

        if (string.IsNullOrWhiteSpace(AppVersion))
        {
            throw new InvalidOperationException("AppVersion is required for SEC user agent identification.");
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            throw new InvalidOperationException("Email is required for SEC user agent identification.");
        }

        // Build UserAgent from components
        UserAgent = $"{AppName}/{AppVersion} ({Email})";
    }
}
