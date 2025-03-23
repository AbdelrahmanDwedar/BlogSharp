using BlogSharp.Controllers;
using BlogSharp.Data;
using BlogSharp.Entities;
using BlogSharp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace BlogSharp.Tests.Controllers;

public class UserControllerTests
{
	private readonly Mock<BlogDbContext> _mockDbContext;
	private readonly Mock<ICache> _mockCache;
	private readonly UserController _controller;

	public UserControllerTests()
	{
		_mockDbContext = new Mock<BlogDbContext>(new DbContextOptions<BlogDbContext>());
		_mockCache = new Mock<ICache>();
		_controller = new UserController(_mockDbContext.Object, _mockCache.Object);
	}

	[Fact]
	public async Task GetAllUsers_ReturnsUsers()
	{
		// Arrange
		var users = new List<User>
		{
			new User
			{
				Name = "John Doe",
				Email = "john.doe@example.com",
				Password = "Password123!",
				Phone = "1234567890"
			}
		};
		_mockDbContext.Setup(db => db.Set<User>()).ReturnsDbSet(users);

		// Act
		var result = await _controller.GetAllUsers();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Equal(users, okResult.Value);
	}

	[Fact]
	public async Task GetUserById_ReturnsUser_WhenExists()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var user = new User
		{
			Id = userId,
			Name = "John Doe",
			Email = "john.doe@example.com",
			Password = "Password123!",
			Phone = "1234567890"
		};
		_mockCache.Setup(c => c.GetAsync<User>($"User_{userId}")).ReturnsAsync(user);

		// Act
		var result = await _controller.GetUserById(userId);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Equal(user, okResult.Value);
	}

	[Fact]
	public async Task GetUserById_ReturnsNotFound_WhenNotExists()
	{
		// Arrange
		var userId = Guid.NewGuid();
		_mockCache.Setup(c => c.GetAsync<User>($"User_{userId}")).ReturnsAsync((User?)null);
		_mockDbContext.Setup(db => db.Set<User>()).ReturnsDbSet(new List<User>());

		// Act
		var result = await _controller.GetUserById(userId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}
}
