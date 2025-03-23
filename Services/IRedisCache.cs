using System.Threading.Tasks;

namespace BlogSharp.Services;

public interface IRedisCache
{
	Task<T?> GetAsync<T>(string key);
	Task SetAsync<T>(string key, T value, TimeSpan expiration);
	Task RemoveAsync(string key);
}
