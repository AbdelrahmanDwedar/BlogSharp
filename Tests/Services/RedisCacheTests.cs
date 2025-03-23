using BlogSharp.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BlogSharp.Tests.Services;

public class RedisCacheTests
{
	private readonly Mock<IDistributedCache> _mockCache;
	private readonly RedisCache _redisCache;

	public RedisCacheTests()
	{
		_mockCache = new Mock<IDistributedCache>();
		_redisCache = new RedisCache(_mockCache.Object);
	}

	[Fact]
	public async Task GetAsync_ReturnsValue_WhenKeyExists()
	{
		// Arrange
		var key = "testKey";
		var value = "testValue";
		_mockCache.Setup(c => c.GetAsync(key, default))
			.ReturnsAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));

		// Act
		var result = await _redisCache.GetAsync<string>(key);

		// Assert
		Assert.Equal(value, result);
	}

	[Fact]
	public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExist()
	{
		// Arrange
		var key = "nonExistentKey";
		_mockCache.Setup(c => c.GetAsync(key, default))
			.ReturnsAsync((byte[]?)null);

		// Act
		var result = await _redisCache.GetAsync<string>(key);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task SetAsync_SetsValueInCache()
	{
		// Arrange
		var key = "testKey";
		var value = "testValue";
		var expiration = TimeSpan.FromMinutes(5);

		// Act
		await _redisCache.SetAsync(key, value, expiration);

		// Assert
		_mockCache.Verify(c => c.SetAsync(
			key,
			It.IsAny<byte[]>(),
			It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
			default
		), Times.Once);
	}

	[Fact]
	public async Task RemoveAsync_RemovesValueFromCache()
	{
		// Arrange
		var key = "testKey";

		// Act
		await _redisCache.RemoveAsync(key);

		// Assert
		_mockCache.Verify(c => c.RemoveAsync(key, default), Times.Once);
	}
}
