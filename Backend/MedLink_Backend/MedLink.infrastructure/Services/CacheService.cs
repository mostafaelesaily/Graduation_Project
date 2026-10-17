using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
namespace MedLink.infrastructure.Services
{
    public class CacheService : ICacheService
    {
        public CacheService
            (
            ILogger<CacheService> logger, 
            IDistributedCache cache,
            IConnectionMultiplexer redis
            )
        {
            this.logger = logger;
            this.cache = cache;
            this.redis = redis; 

        }
        private readonly ILogger<CacheService> logger;
        private readonly IDistributedCache cache;
        private readonly IConnectionMultiplexer redis;

        public async Task<bool> ExistsAsync(string key)
        {
            return await cache.GetStringAsync(key) != null;
        }

        public async Task<T?> GetAsync<T>(string Key)
        {
            var value = await cache.GetStringAsync(Key);
            return value == null ? 
           default : JsonConvert.DeserializeObject<T>(value);
        }

        public Task setAsync<T>(string key, T value, TimeSpan? slidingExpiration = null, TimeSpan? absoluteExpiration = null)
        {
            var options = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = slidingExpiration ?? TimeSpan.FromMinutes(5),
                AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromHours(1)
            };
            var josn = JsonConvert.SerializeObject(value);
            return cache.SetStringAsync(key, josn, options);
        }

        public async Task RemoveAsync(string Pattern)
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var database = redis.GetDatabase();
            foreach (var key in server.Keys(pattern: $"{Pattern}*"))
            {
                await database.KeyDeleteAsync(key);
            }

        }

        public Task<T?> GetOrSetAsync<T>
            (string key, Func<Task<T?>> factory,
            TimeSpan? slidingExpiration = null,
            TimeSpan? absoluteExpiration = null)
        {
          var value = GetAsync<T>(key);
            if (value != null)
            {
                return value;
            }
            var newValue = factory();
            setAsync(key, newValue, slidingExpiration, absoluteExpiration);
            return newValue;
        }

        

       }
}
