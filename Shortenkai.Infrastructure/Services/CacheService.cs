using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Shortenkai.Infrastructure.Services
{
    public class CacheService(IDistributedCache cache)
    {
        private static DistributedCacheEntryOptions CreateOptions(TimeSpan ttl) =>
            new DistributedCacheEntryOptions().SetAbsoluteExpiration(ttl);

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, json, CreateOptions(ttl));
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var json = await cache.GetStringAsync(key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }

        public async Task RemoveAsync(string key) =>
            await cache.RemoveAsync(key);
    }
}
