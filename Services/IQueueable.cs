namespace BlogSharp.Services;

public interface IQueueable
{
	Task EnqueueAsync<T>(string queueName, T message);
	Task<T?> DequeueAsync<T>(string queueName);
}
