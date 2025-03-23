using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BlogSharp.Services;

public class RabbitMQQueue : IQueueable
{
	private readonly IConnection _connection;
	private readonly IModel _channel;

	public RabbitMQQueue(IConnection connection)
	{
		_connection = connection;
		_channel = _connection.CreateModel();
	}

	public Task EnqueueAsync<T>(string queueName, T message)
	{
		_channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

		var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
		_channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);

		return Task.CompletedTask;
	}

	public Task<T?> DequeueAsync<T>(string queueName)
	{
		_channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

		var consumer = new EventingBasicConsumer(_channel);
		var tcs = new TaskCompletionSource<T?>();

		consumer.Received += (model, ea) =>
		{
			var body = ea.Body.ToArray();
			var message = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(body));
			tcs.SetResult(message);
			_channel.BasicAck(ea.DeliveryTag, false);
		};

		_channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

		return tcs.Task;
	}
}
