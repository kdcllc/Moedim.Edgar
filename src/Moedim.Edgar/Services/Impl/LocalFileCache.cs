using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Moedim.Edgar.Services.Impl;

/// <summary>
/// Local file-based cache implementation for SEC EDGAR data
/// </summary>
public class LocalFileCache : ICacheService
{
    private readonly string _cacheDirectory;
    private readonly ILogger<LocalFileCache>? _logger;

    /// <summary>
    /// Initializes a new instance of the LocalFileCache
    /// </summary>
    /// <param name="cacheDirectory">Optional cache directory path. Defaults to .edgar_cache in temp folder.</param>
    /// <param name="logger">Optional logger instance</param>
    public LocalFileCache(string? cacheDirectory = null, ILogger<LocalFileCache>? logger = null)
    {
        _cacheDirectory = cacheDirectory ?? Path.Combine(Path.GetTempPath(), ".edgar_cache");
        _logger = logger;

        // Ensure cache directory exists
        if (!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var filePath = GetCacheFilePath(key);

            if (!File.Exists(filePath))
            {
                return null;
            }

            // Check if cache is expired
            var metadata = await ReadMetadataAsync(filePath);
            if (metadata?.ExpiresAt < DateTime.UtcNow)
            {
                File.Delete(filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var cacheEntry = JsonSerializer.Deserialize<CacheEntry<T>>(json);

            return cacheEntry?.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to read from cache for key: {Key}", key);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            var cacheEntry = new CacheEntry<T>
            {
                Value = value,
                ExpiresAt = DateTime.UtcNow.Add(expiration)
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            var json = JsonSerializer.Serialize(cacheEntry, options);

            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to write to cache for key: {Key}", key);
        }
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        try
        {
            var filePath = GetCacheFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to remove cache entry for key: {Key}", key);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ClearAsync()
    {
        try
        {
            if (Directory.Exists(_cacheDirectory))
            {
                foreach (var file in Directory.GetFiles(_cacheDirectory))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to delete cache file: {File}", file);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to clear cache directory");
        }

        return Task.CompletedTask;
    }

    private string GetCacheFilePath(string key)
    {
        // Create a safe filename from the key using hash
        var hash = ComputeHash(key);
        return Path.Combine(_cacheDirectory, $"{hash}.json");
    }

    private async Task<CacheMetadata?> ReadMetadataAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var metadata = JsonSerializer.Deserialize<CacheMetadata>(json);
            return metadata;
        }
        catch
        {
            return null;
        }
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private class CacheEntry<T>
    {
        public T? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private class CacheMetadata
    {
        public DateTime ExpiresAt { get; set; }
    }
}
