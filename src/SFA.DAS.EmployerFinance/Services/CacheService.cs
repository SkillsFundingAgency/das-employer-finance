using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SFA.DAS.EmployerFinance.Interfaces;

namespace SFA.DAS.EmployerFinance.Services;

public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<(bool Found, T? Value)> TryGetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await distributedCache.GetAsync(key, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return (false, default);
        }

        var stored = System.Text.Encoding.UTF8.GetString(bytes);
        if (string.IsNullOrEmpty(stored))
        {
            return (false, default);
        }

        var value = Deserialize<T>(stored);
        return (true, value);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var serialized = Serialize(value);
        var bytes = System.Text.Encoding.UTF8.GetBytes(serialized);
        var options = new DistributedCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        await distributedCache.SetAsync(key, bytes, options, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return distributedCache.RemoveAsync(key, cancellationToken);
    }

    private static string Serialize<T>(T value)
    {
        return value switch
        {
            string s => s,
            decimal d => d.ToString("G", CultureInfo.InvariantCulture),
            _ => JsonSerializer.Serialize(value, JsonOptions)
        };
    }

    private static T? Deserialize<T>(string stored)
    {
        if (typeof(T) == typeof(string))
        {
            return (T)(object)stored;
        }

        if (typeof(T) == typeof(decimal))
        {
            return decimal.TryParse(stored, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? (T)(object)d
                : default;
        }

        return JsonSerializer.Deserialize<T>(stored, JsonOptions);
    }
}
