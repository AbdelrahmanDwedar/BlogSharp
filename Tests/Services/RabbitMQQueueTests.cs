using BlogSharp.Services;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BlogSharp.Tests.Services;

public class RabbitMQQueueTests
{
	private readonly Mock<IModel> _mockChannel;
	private readonly RabbitMQQueue _rabbitMQQueue;

	public RabbitMQQueueTests()
	{
		_mockChannel = new Mock<IModel>();
		var mockConnection = new Mock<IConnection>();
		mockConnection.Setup(c => c.CreateModel()).Returns(_mockChannel.Object);
		_rabbitMQQueue = new RabbitMQQueue(mockConnection.Object);
	}

	[Fact]
	public async Task EnqueueAsync_PublishesMessageToQueue()
	{
		// Arrange
		var queueName = "testQueue";
		var message = "testMessage";

		// Act
		await _rabbitMQQueue.EnqueueAsync(queueName, message);

		// Assert
		_mockChannel.Verify(c => c.BasicPublish(
			"",
			queueName,
			null,
			It.IsAny<byte[]>()
		), Times.Once);
	}

	[Fact]
	public async Task DequeueAsync_ReturnsMessageFromQueue()
	{
		// Arrange
		var queueName = "testQueue";
		var message = "testMessage";
		var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
		var consumer = new EventingBasicConsumer(_mockChannel.Object);

		_mockChannel.Setup(c => c.BasicConsume(queueName, false, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
			.Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, cons) =>
			{
				consumer.Received += (model, ea) =>
				{
					consumer.HandleBasicDeliver("", ea.DeliveryTag, false, "", "", null, body);
				};
			});

		// Act
		var result = await _rabbitMQQueue.DequeueAsync<string>(queueName);

		// Assert
		Assert.Equal(message, result);
	}
}
