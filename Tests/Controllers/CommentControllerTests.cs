using BlogSharp.Controllers;
using BlogSharp.Data;
using BlogSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BlogSharp.Tests.Controllers;

public class CommentControllerTests
{
	private readonly Mock<BlogDbContext> _mockDbContext;
	private readonly CommentController _controller;

	public CommentControllerTests()
	{
		_mockDbContext = new Mock<BlogDbContext>(new DbContextOptions<BlogDbContext>());
		_controller = new CommentController(_mockDbContext.Object);
	}

	[Fact]
	public async Task GetCommentsByBlog_ReturnsComments()
	{
		// Arrange
		var blogId = Guid.NewGuid();
		var comments = new List<Comment> { new Comment(null!, null!, "Test Comment") { BlogId = blogId } };
		_mockDbContext.Setup(db => db.Comments.Where(c => c.Blog.Id == blogId)).ReturnsDbSet(comments);

		// Act
		var result = await _controller.GetCommentsByBlog(blogId);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		Assert.Equal(comments, okResult.Value);
	}

	[Fact]
	public async Task AddComment_AddsComment()
	{
		// Arrange
		var newComment = new Comment(null!, null!, "New Comment");

		// Act
		var result = await _controller.AddComment(newComment);

		// Assert
		Assert.IsType<CreatedAtActionResult>(result.Result);
		_mockDbContext.Verify(db => db.Comments.Add(newComment), Times.Once);
	}

	[Fact]
	public async Task UpdateComment_UpdatesComment_WhenExists()
	{
		// Arrange
		var commentId = Guid.NewGuid();
		var existingComment = new Comment(null!, null!, "Old Content") { Id = commentId };
		var updatedComment = new Comment(null!, null!, "New Content");
		_mockDbContext.Setup(db => db.Comments.FindAsync(commentId)).ReturnsAsync(existingComment);

		// Act
		var result = await _controller.UpdateComment(commentId, updatedComment);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Equal("New Content", existingComment.Content);
	}

	[Fact]
	public async Task DeleteComment_RemovesComment_WhenExists()
	{
		// Arrange
		var commentId = Guid.NewGuid();
		var comment = new Comment(null!, null!, "Test Comment") { Id = commentId };
		_mockDbContext.Setup(db => db.Comments.FindAsync(commentId)).ReturnsAsync(comment);

		// Act
		var result = await _controller.DeleteComment(commentId);

		// Assert
		Assert.IsType<NoContentResult>(result);
		_mockDbContext.Verify(db => db.Comments.Remove(comment), Times.Once);
	}
}
