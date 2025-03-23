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
	private readonly Mock<BlogDbContext> _mockDbContext;
	private readonly Mock<ICache> _mockCache;
	private readonly Mock<IQueueable> _mockQueue;
	private readonly BlogController _controller;

	public BlogControllerTests()
	{
		_mockDbContext = new Mock<BlogDbContext>(new DbContextOptions<BlogDbContext>());
		_mockCache = new Mock<ICache>();
		_mockQueue = new Mock<IQueueable>();
		_controller = new BlogController(_mockDbContext.Object, _mockCache.Object, _mockQueue.Object);
	}

	[Fact]
	public async Task GetAllBlogs_ReturnsBlogs()
	{
		// Arrange
		var blogs = new List<Blog> { new Blog { Title = "Test Blog" } };
		_mockDbContext.Setup(db => db.Blogs.Include(b => b.User)).ReturnsDbSet(blogs);

		// Act
		var result = await _controller.GetAllBlogs();

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Equal(blogs, okResult.Value);
	}

	[Fact]
	public async Task GetBlogById_ReturnsBlog_WhenExists()
	{
		// Arrange
		var blogId = Guid.NewGuid();
		var blog = new Blog { Id = blogId, Title = "Test Blog" };
		_mockCache.Setup(c => c.GetAsync<Blog>($"Blog_{blogId}")).ReturnsAsync(blog);

		// Act
		var result = await _controller.GetBlogById(blogId);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Equal(blog, okResult.Value);
	}

	[Fact]
	public async Task GetBlogById_ReturnsNotFound_WhenNotExists()
	{
		// Arrange
		var blogId = Guid.NewGuid();
		_mockCache.Setup(c => c.GetAsync<Blog>($"Blog_{blogId}")).ReturnsAsync((Blog?)null);
		_mockDbContext.Setup(db => db.Blogs.FindAsync(blogId)).ReturnsAsync((Blog?)null);

		// Act
		var result = await _controller.GetBlogById(blogId);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task AddNewBlog_EnqueuesBlog()
	{
		// Arrange
		var newBlog = new Blog { Title = "New Blog" };

		// Act
		var result = await _controller.AddNewBlog(newBlog);

		// Assert
		Assert.IsType<AcceptedResult>(result);
		_mockQueue.Verify(q => q.EnqueueAsync("BlogQueue", newBlog), Times.Once);
	}

	[Fact]
	public async Task UpdateBlog_UpdatesBlog_WhenExists()
	{
		// Arrange
		var blogId = Guid.NewGuid();
		var existingBlog = new Blog { Id = blogId, Title = "Old Title" };
		var updatedBlog = new Blog { Title = "New Title" };
		_mockDbContext.Setup(db => db.Blogs.FindAsync(blogId)).ReturnsAsync(existingBlog);

		// Act
		var result = await _controller.UpdateBlog(blogId, updatedBlog);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Equal("New Title", existingBlog.Title);
	}

	[Fact]
	public async Task DeleteBlog_RemovesBlog_WhenExists()
	{
		// Arrange
		var blogId = Guid.NewGuid();
		var blog = new Blog { Id = blogId };
		_mockDbContext.Setup(db => db.Blogs.FindAsync(blogId)).ReturnsAsync(blog);

		// Act
		var result = await _controller.DeleteBlog(blogId);

		// Assert
		Assert.IsType<NoContentResult>(result);
		_mockDbContext.Verify(db => db.Blogs.Remove(blog), Times.Once);
	}
}
