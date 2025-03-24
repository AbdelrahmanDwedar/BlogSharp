using BlogSharp.Controllers;
using BlogSharp.Data;
using BlogSharp.Entities;
using BlogSharp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlogSharp.Tests.Controllers;

public class BlogControllerTests
{
    private BlogDbContext _context;
    private readonly Mock<ICache> _mockCache;
    private readonly Mock<IQueueable> _mockQueue;
    private BlogController _controller;

    public BlogControllerTests()
    {
        _mockCache = new Mock<ICache>();
        _mockQueue = new Mock<IQueueable>();
        ResetDatabase();
    }

    private void ResetDatabase()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique database for each test
            .Options;

        _context = new BlogDbContext(options);
        _controller = new BlogController(_context, _mockCache.Object, _mockQueue.Object);
    }

    [Fact]
    public async Task GetAllBlogs_ReturnsOkResult_WithBlogs()
    {
        // Arrange
        ResetDatabase();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "testuser@example.com",
            Password = "password123",
            Phone = "1234567890",
        };
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = "Test Blog",
            Content = "Test Content",
            User = user,
        };
        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAllBlogs();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var blogs = Assert.IsAssignableFrom<IEnumerable<Blog>>(okResult.Value);
        Assert.Single(blogs);
    }

    [Fact]
    public async Task GetBlogById_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        // Arrange
        var blogId = Guid.NewGuid();

        // Act
        var result = await _controller.GetBlogById(blogId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task AddNewBlog_ReturnsAccepted_WhenBlogIsQueued()
    {
        // Arrange
        ResetDatabase();
        var newBlog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = "New Blog",
            Content = "New Content",
            User = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "password123",
                Phone = "1234567890",
            },
        };
        _mockQueue
            .Setup(q => q.EnqueueAsync("BlogQueue", It.IsAny<Blog>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddNewBlog(newBlog);

        // Assert
        _mockQueue.Verify(q => q.EnqueueAsync("BlogQueue", newBlog), Times.Once); // Verify enqueue was called
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        Assert.NotNull(acceptedResult); // Ensure the response is not null
        Assert.Equal(202, acceptedResult.StatusCode);
    }

    [Fact]
    public async Task UpdateBlog_ReturnsNotFound_WhenBlogDoesNotExist()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var updatedBlog = new Blog
        {
            Title = "Updated Title",
            Content = "Updated Content",
            User = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "password123",
                Phone = "1234567890",
            },
        };

        // Act
        var result = await _controller.UpdateBlog(blogId, updatedBlog);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteBlog_ReturnsNoContent_WhenBlogIsDeleted()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "testuser@example.com",
            Password = "password123",
            Phone = "1234567890",
        };
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = "Test Blog",
            Content = "Test Content",
            User = user,
        };
        _context.Blogs.Add(blog);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteBlog(blog.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
