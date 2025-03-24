using BlogSharp.Controllers;
using BlogSharp.Data;
using BlogSharp.Dtos;
using BlogSharp.Entities;
using BlogSharp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlogSharp.Tests.Controllers;

public class UserControllerTests
{
	private readonly BlogDbContext _context;
	private readonly Mock<ICache> _mockCache;
	private readonly UserController _controller;

	public UserControllerTests()
	{
		var options = new DbContextOptionsBuilder<BlogDbContext>()
	.UseInMemoryDatabase(databaseName: "TestDatabase")
	.Options;

		_context = new BlogDbContext(options);
		_mockCache = new Mock<ICache>();
		_controller = new UserController(_context, _mockCache.Object);
	}

	// Clear the database before each test
	private void ClearDatabase()
	{
		_context.Users.RemoveRange(_context.Users);
		_context.SaveChanges();
	}

	[Fact]
	public async Task GetAllUsers_ReturnsOkResult_WithUsers()
	{
		// Arrange
		ClearDatabase(); // Ensure the database is empty before the test

		_context.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			Name = "John Doe",
			Email = "john@example.com",
			Password = "Password123!",
			Phone = "1234567890",
			CreatedAt = DateTime.UtcNow
		});
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.GetAllUsers();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var returnedUsers = Assert.IsType<List<User>>(okResult.Value);
		Assert.Single(returnedUsers);
	}

	[Fact]
	public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
	{
		// Arrange
		var userId = Guid.NewGuid();

		// Act
		var result = await _controller.GetUserById(userId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task AddNewUserAsync_ReturnsCreatedAtActionResult()
	{
		// Arrange
		var newUser = new CreateUser
		{
			Name = "Jane Doe",
			Email = "jane@example.com",
			Password = "Password123!",
			Phone = "1234567890"
		};

		// Act
		var result = await _controller.AddNewUserAsync(newUser);

		// Assert
		var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
		var createdUser = Assert.IsType<User>(createdResult.Value);
		Assert.Equal(newUser.Name, createdUser.Name);
	}

	[Fact]
	public async Task UpdateUser_ReturnsNoContent_WhenUserIsUpdated()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var existingUser = new User
		{
			Id = userId,
			Name = "John Doe",
			Email = "john@example.com",
			Password = "Password123!",
			Phone = "1234567890",
			CreatedAt = DateTime.UtcNow
		};
		_context.Users.Add(existingUser);
		await _context.SaveChangesAsync();

		var updatedUser = new UpdateUser { Name = "John Updated" };

		// Act
		var result = await _controller.UpdateUser(userId, updatedUser);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Equal(updatedUser.Name, existingUser.Name);
	}

	[Fact]
	public async Task DeleteUser_ReturnsNoContent_WhenUserIsDeleted()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var existingUser = new User
		{
			Id = userId,
			Name = "John Doe",
			Email = "john@example.com",
			Password = "Password123!",
			Phone = "1234567890",
			CreatedAt = DateTime.UtcNow
		};
		_context.Users.Add(existingUser);
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.DeleteUser(userId);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Null(await _context.Users.FindAsync(userId));
	}
}
