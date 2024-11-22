using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace RedisDemo;

public static class DistributedCacheExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        AllowTrailingCommas = true
    };

    public static async Task<T?> GetOrSetAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> task)
    {
        if (cache.TryGetValue(key, out T? value) && value is not null)
        {
            Console.WriteLine("Fetching data from cache");
            return value;
        }

        value = await task();

        if (value is null) return value;

        await cache.SetAsync<T>(key, value);

        return value;
    }

    private static async Task SetAsync<T>(this IDistributedCache cache, string key, T value)
    {
        var options = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)).SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, SerializerOptions));
        
        Console.WriteLine("Caching data");
        
        await cache.SetAsync(key, bytes, options);
    }

    private static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
    {
        var val = cache.Get(key);

        value = default;
        
        if (val is null) return false;
        
        value = JsonSerializer.Deserialize<T>(val, SerializerOptions);
            
        return true;

    }
}