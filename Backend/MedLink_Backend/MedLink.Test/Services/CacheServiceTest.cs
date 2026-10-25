using System.Text;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MedLink.Test.Services
{
    public class CacheServiceTest
    {
        private readonly Mock<ILogger<CacheService>> loggerMock = new();
        private readonly Mock<IDistributedCache> cacheMock = new();
        private readonly Mock<IConnectionMultiplexer> redisMock = new();
        private readonly CacheService sut;

        public CacheServiceTest()
        {
            sut = new CacheService(loggerMock.Object, cacheMock.Object, redisMock.Object);
        }

        private class SampleData
        {
            public string Name { get; set; } = string.Empty;
        }

        // ---------- ExistsAsync ----------

        [Fact]
        public async Task ExistsAsync_KeyPresent_ReturnsTrue()
        {
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes("value"));

            var result = await sut.ExistsAsync("key");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_KeyMissing_ReturnsFalse()
        {
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var result = await sut.ExistsAsync("key");

            Assert.False(result);
        }

        // ---------- GetAsync ----------

        [Fact]
        public async Task GetAsync_KeyMissing_ReturnsDefault()
        {
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var result = await sut.GetAsync<SampleData>("key");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAsync_KeyPresent_ReturnsDeserializedValue()
        {
            var data = new SampleData { Name = "Lab" };
            var json = JsonConvert.SerializeObject(data);
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(json));

            var result = await sut.GetAsync<SampleData>("key");

            Assert.NotNull(result);
            Assert.Equal("Lab", result!.Name);
        }

        // ---------- setAsync ----------

        [Fact]
        public async Task setAsync_SerializesValueAndStoresInCache()
        {
            var data = new SampleData { Name = "Lab" };
            byte[]? storedBytes = null;
            cacheMock.Setup(c => c.SetAsync(
                    "key",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, bytes, _, _) => storedBytes = bytes)
                .Returns(Task.CompletedTask);

            await sut.setAsync("key", data);

            Assert.NotNull(storedBytes);
            var storedJson = Encoding.UTF8.GetString(storedBytes!);
            Assert.Equal(JsonConvert.SerializeObject(data), storedJson);
        }

        [Fact]
        public async Task setAsync_UsesProvidedExpirations()
        {
            DistributedCacheEntryOptions? capturedOptions = null;
            cacheMock.Setup(c => c.SetAsync(
                    "key",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, _, options, _) => capturedOptions = options)
                .Returns(Task.CompletedTask);

            await sut.setAsync("key", new SampleData(), slidingExpiration: TimeSpan.FromMinutes(2), absoluteExpiration: TimeSpan.FromMinutes(10));

            Assert.Equal(TimeSpan.FromMinutes(2), capturedOptions!.SlidingExpiration);
            Assert.Equal(TimeSpan.FromMinutes(10), capturedOptions!.AbsoluteExpirationRelativeToNow);
        }

        // ---------- RemoveAsync ----------

        [Fact]
        public async Task RemoveAsync_DeletesAllKeysMatchingPattern()
        {
            var endpoint = new System.Net.DnsEndPoint("localhost", 6379);
            var serverMock = new Mock<IServer>();
            var databaseMock = new Mock<IDatabase>();
            var matchingKeys = new[] { (RedisKey)"Get_ProviderRequest_1", (RedisKey)"Get_ProviderRequest_2" };

            redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>())).Returns(new System.Net.EndPoint[] { endpoint });
            redisMock.Setup(r => r.GetServer(endpoint, null)).Returns(serverMock.Object);
            redisMock.Setup(r => r.GetDatabase(-1, null)).Returns(databaseMock.Object);
            serverMock.Setup(s => s.Keys(
                    It.IsAny<int>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(matchingKeys);
            databaseMock.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

            await sut.RemoveAsync("Get_ProviderRequest");

            foreach (var key in matchingKeys)
            {
                databaseMock.Verify(d => d.KeyDeleteAsync(key, It.IsAny<CommandFlags>()), Times.Once);
            }
        }

        // ---------- GetOrSetAsync ----------

        [Fact]
        public async Task GetOrSetAsync_CacheMiss_InvokesFactoryAndCachesResult()
        {
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);
            byte[]? storedBytes = null;
            cacheMock.Setup(c => c.SetAsync(
                    "key",
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((_, bytes, _, _) => storedBytes = bytes)
                .Returns(Task.CompletedTask);
            var factoryInvoked = false;
            Func<Task<SampleData?>> factory = () =>
            {
                factoryInvoked = true;
                return Task.FromResult<SampleData?>(new SampleData { Name = "Fresh" });
            };

            var result = await sut.GetOrSetAsync("key", factory);

            Assert.True(factoryInvoked);
            Assert.Equal("Fresh", result!.Name);
            Assert.NotNull(storedBytes);
            Assert.Equal(JsonConvert.SerializeObject(result), Encoding.UTF8.GetString(storedBytes!));
        }

        [Fact]
        public async Task GetOrSetAsync_CacheHit_ReturnsCachedValueWithoutInvokingFactory()
        {
            var cached = new SampleData { Name = "Cached" };
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cached)));
            var factoryInvoked = false;
            Func<Task<SampleData?>> factory = () =>
            {
                factoryInvoked = true;
                return Task.FromResult<SampleData?>(new SampleData { Name = "Should not be used" });
            };

            var result = await sut.GetOrSetAsync("key", factory);

            Assert.False(factoryInvoked);
            Assert.Equal("Cached", result!.Name);
        }

        [Fact]
        public async Task GetOrSetAsync_FactoryReturnsNull_DoesNotCacheNull()
        {
            cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);
            Func<Task<SampleData?>> factory = () => Task.FromResult<SampleData?>(null);

            var result = await sut.GetOrSetAsync("key", factory);

            Assert.Null(result);
            cacheMock.Verify(c => c.SetAsync(
                "key", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
