using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FebroFlow.Business.Services;

/// <summary>
/// Interface for caching service that abstracts both in-memory and distributed caching
/// </summary>
public interface ICachingService
{
    /// <summary>
    /// Get a value from the cache
    /// </summary>
    /// <typeparam name="T">Type of value to get</typeparam>
    /// <param name="key">Cache key</param>
    /// <returns>Cached value or default if not found</returns>
    Task<T?> GetAsync<T>(string key);
    
    /// <summary>
    /// Set a value in the cache
    /// </summary>
    /// <typeparam name="T">Type of value to set</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="absoluteExpiration">Absolute expiration time</param>
    /// <param name="slidingExpiration">Sliding expiration time</param>
    /// <returns>Task representing the operation</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    
    /// <summary>
    /// Remove a value from the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Task representing the operation</returns>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Get a value from the cache or compute it if not present
    /// </summary>
    /// <typeparam name="T">Type of value to get</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="valueFactory">Function to compute the value if not in cache</param>
    /// <param name="absoluteExpiration">Absolute expiration time</param>
    /// <param name="slidingExpiration">Sliding expiration time</param>
    /// <returns>Value from cache or computed</returns>
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null);
    
    /// <summary>
    /// Refresh the sliding expiration of a cached item
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>Task representing the operation</returns>
    Task RefreshAsync(string key);
    
    /// <summary>
    /// Check if a key exists in the cache
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <returns>True if key exists in cache</returns>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Clear the entire cache
    /// </summary>
    /// <returns>Task representing the operation</returns>
    Task ClearAsync();
}

/// <summary>
/// Implementation of caching service that supports both in-memory and distributed caching
/// </summary>
public class CachingService : ICachingService
{
    private readonly IMemoryCache? _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<CachingService> _logger;
    private readonly bool _useDistributedCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly TimeSpan _defaultAbsoluteExpiration;
    private readonly TimeSpan _defaultSlidingExpiration;
    
    public CachingService(
        IMemoryCache? memoryCache,
        IDistributedCache? distributedCache,
        IConfiguration configuration,
        ILogger<CachingService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        
        _useDistributedCache = configuration.GetValue<bool>("FebroFlow:UseDistributedCache", false);
        
        _defaultAbsoluteExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<double>("FebroFlow:Cache:DefaultAbsoluteExpirationMinutes", 30));
        
        _defaultSlidingExpiration = TimeSpan.FromMinutes(
            configuration.GetValue<double>("FebroFlow:Cache:DefaultSlidingExpirationMinutes", 10));
    }
    
    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_useDistributedCache && _distributedCache != null)
            {
                var cachedBytes = await _distributedCache.GetAsync(key);
                if (cachedBytes == null || cachedBytes.Length == 0)
                {
                    return default;
                }
                
                var cachedJson = Encoding.UTF8.GetString(cachedBytes);
                return JsonConvert.DeserializeObject<T>(cachedJson);
            }
            else if (_memoryCache != null)
            {
                return _memoryCache.TryGetValue(key, out var value) ? (T)value : default;
            }
            
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key {Key}", key);
            return default;
        }
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        try
        {
            absoluteExpiration ??= _defaultAbsoluteExpiration;
            slidingExpiration ??= _defaultSlidingExpiration;
            
            if (_useDistributedCache && _distributedCache != null)
            {
                var options = new DistributedCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }
                
                var jsonValue = JsonConvert.SerializeObject(value);
                var byteValue = Encoding.UTF8.GetBytes(jsonValue);
                
                await _distributedCache.SetAsync(key, byteValue, options);
            }
            else if (_memoryCache != null)
            {
                var options = new MemoryCacheEntryOptions();
                
                if (absoluteExpiration.HasValue)
                {
                    options.SetAbsoluteExpiration(absoluteExpiration.Value);
                }
                
                if (slidingExpiration.HasValue)
                {
                    options.SetSlidingExpiration(slidingExpiration.Value);
                }
                
                _memoryCache.Set(key, value, options);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        try
        {
            if (_useDistributedCache && _distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key);
            }
            else if (_memoryCache != null)
            {
                _memoryCache.Remove(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key {Key}", key);
        }
    }
    
    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        try
        {
            var value = await GetAsync<T>(key);
            if (value != null && !EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                return value;
            }
            
            // Ensure only one thread is computing the value
            var lockObj = _locks.GetOrAdd(key, k => new SemaphoreSlim(1, 1));
            
            await lockObj.WaitAsync();
            try
            {
                // Double-check after acquiring the lock
                value = await GetAsync<T>(key);
                if (value != null && !EqualityComparer<T>.Default.Equals(value, default(T)))
                {
                    return value;
                }
                
                // Compute the value
                value = await valueFactory();
                
                // Cache the value
                await SetAsync(key, value, absoluteExpiration, slidingExpiration);
                
                return value;
            }
            finally
            {
                lockObj.Release();
                
                // Clean up the lock if no longer needed
                if (_locks.TryGetValue(key, out var semaphore) && semaphore.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                    semaphore.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating value in cache for key {Key}", key);
            
            // If caching fails, compute the value directly
            return await valueFactory();
        }
    }
    
    public async Task RefreshAsync(string key)
    {
        try
        {
            if (_useDistributedCache && _distributedCache != null)
            {
                await _distributedCache.RefreshAsync(key);
            }
            else if (_memoryCache != null)
            {
                // Memory cache doesn't have a direct Refresh method
                // Touch the item if it exists
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    // Re-set the same value to refresh sliding expiration
                    _memoryCache.Set(key, value, new MemoryCacheEntryOptions
                    {
                        SlidingExpiration = _defaultSlidingExpiration
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache for key {Key}", key);
        }
    }
    
    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            if (_useDistributedCache && _distributedCache != null)
            {
                var value = await _distributedCache.GetAsync(key);
                return value != null && value.Length > 0;
            }
            else if (_memoryCache != null)
            {
                return _memoryCache.TryGetValue(key, out _);
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in cache for key {Key}", key);
            return false;
        }
    }
    
    public async Task ClearAsync()
    {
        try
        {
            if (_memoryCache != null && _memoryCache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
            
            // Distributed cache doesn't have a Clear method
            // This is a limitation, but in practice distributed caches
            // are often shared resources and shouldn't be cleared entirely
            
            // Clear all locks
            foreach (var kvp in _locks)
            {
                kvp.Value.Dispose();
            }
            _locks.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }
}

/// <summary>
/// Extension method to handle synchronous caching operations
/// </summary>
public static class CachingServiceExtensions
{
    /// <summary>
    /// Get a value from the cache synchronously
    /// </summary>
    public static T? Get<T>(this ICachingService cache, string key)
    {
        return cache.GetAsync<T>(key).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Set a value in the cache synchronously
    /// </summary>
    public static void Set<T>(this ICachingService cache, string key, T value, 
        TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        cache.SetAsync(key, value, absoluteExpiration, slidingExpiration).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Remove a value from the cache synchronously
    /// </summary>
    public static void Remove(this ICachingService cache, string key)
    {
        cache.RemoveAsync(key).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Get a value from the cache or compute it if not present synchronously
    /// </summary>
    public static T GetOrCreate<T>(this ICachingService cache, string key, Func<T> valueFactory, 
        TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        return cache.GetOrCreateAsync(key, async () => await Task.FromResult(valueFactory()), 
            absoluteExpiration, slidingExpiration).GetAwaiter().GetResult();
    }
}