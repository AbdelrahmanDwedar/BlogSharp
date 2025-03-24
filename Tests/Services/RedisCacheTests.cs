using System.Text;
using System.Text.Json;
using BlogSharp.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

public class RedisCacheTests
{
    private readonly Mock<IDistributedCache> _mockDistributedCache;
    private readonly RedisCache _redisCache;

    public RedisCacheTests()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _redisCache = new RedisCache(_mockDistributedCache.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        var serializedValue = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedValue));
        _mockDistributedCache
            .Setup(cache => cache.GetAsync(key, default))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _redisCache.GetAsync<string>(key);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "non-existent-key";
        _mockDistributedCache
            .Setup(cache => cache.GetAsync(key, default))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _redisCache.GetAsync<string>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_StoresValueSuccessfully()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(5);
        var serializedValue = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

        _mockDistributedCache
            .Setup(cache =>
                cache.SetAsync(
                    key,
                    serializedValue,
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == expiration
                    ),
                    default
                )
            )
            .Returns(Task.CompletedTask);

        // Act
        await _redisCache.SetAsync(key, value, expiration);

        // Assert
        _mockDistributedCache.Verify(
            cache =>
                cache.SetAsync(
                    key,
                    serializedValue,
                    It.Is<DistributedCacheEntryOptions>(options =>
                        options.AbsoluteExpirationRelativeToNow == expiration
                    ),
                    default
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task RemoveAsync_RemovesKeySuccessfully()
    {
        // Arrange
        var key = "test-key";

        _mockDistributedCache
            .Setup(cache => cache.RemoveAsync(key, default))
            .Returns(Task.CompletedTask);

        // Act
        await _redisCache.RemoveAsync(key);

        // Assert
        _mockDistributedCache.Verify(cache => cache.RemoveAsync(key, default), Times.Once);
    }
}
