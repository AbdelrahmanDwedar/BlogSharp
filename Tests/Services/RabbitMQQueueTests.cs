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
	[Fact]
	public async Task EnqueueAsync_ShouldPublishMessageToQueue()
	{
		// Arrange
		var mockConnection = new Mock<IConnection>();
		var mockChannel = new Mock<IModel>();
		mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);

		var queue = new RabbitMQQueue(mockConnection.Object);
		var queueName = "test-queue";
		var message = new { Text = "Hello, World!" };

		// Act
		await queue.EnqueueAsync(queueName, message);

		// Assert
		mockChannel.Verify(c => c.QueueDeclare(queueName, true, false, false, null), Times.Once);
	}

	[Fact]
	public async Task DequeueAsync_ShouldConsumeMessageFromQueue()
	{
		// Arrange
		var mockConnection = new Mock<IConnection>();
		var mockChannel = new Mock<IModel>();
		mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);

		var queue = new RabbitMQQueue(mockConnection.Object);
		var queueName = "test-queue";
		var expectedMessage = new { Text = "Hello, World!" };
		var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedMessage));

		var consumer = new EventingBasicConsumer(mockChannel.Object);
		mockChannel.Setup(c => c.BasicConsume(queueName, false, It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
			.Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>((_, _, _, _, _, _, cons) =>
			{
				consumer = (EventingBasicConsumer)cons;
			});

		// Act
		var dequeueTask = queue.DequeueAsync<object>(queueName);
		consumer.HandleBasicDeliver("", 1, false, "", "", null, messageBody);

		var result = await dequeueTask;

		// Assert
		Assert.NotNull(result);
		Assert.Equal(JsonSerializer.Serialize(expectedMessage), JsonSerializer.Serialize(result));
		mockChannel.Verify(c => c.QueueDeclare(queueName, true, false, false, null), Times.Once);
		mockChannel.Verify(c => c.BasicAck(1, false), Times.Once);
	}
}
