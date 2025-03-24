using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogSharp.Controllers;
using BlogSharp.Data;
using BlogSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BlogSharp.Tests.Controllers;

public class CommentControllerTests
{
    private readonly BlogDbContext _context;
    private readonly CommentController _controller;

    public CommentControllerTests()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BlogDbContext(options);
        _controller = new CommentController(_context);
    }

    [Fact]
    public async Task GetCommentsByBlog_ReturnsComments()
    {
        // Arrange
        var blogId = Guid.NewGuid();
        var blog = new Blog
        {
            Id = blogId,
            Title = "Test Blog",
            Content = "Test Content",
            User = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "TestPassword123",
                Phone = "123-456-7890",
            },
        };
        _context.Add(blog);
        await _context.SaveChangesAsync();

        _context.Comments.AddRange(
            new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    BlogId = blogId,
                    Blog = blog,
                    UserId = Guid.NewGuid(),
                    Content = "Test Comment 1",
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    BlogId = blogId,
                    Blog = blog,
                    UserId = Guid.NewGuid(),
                    Content = "Test Comment 2",
                },
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCommentsByBlog(blogId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedComments = Assert.IsType<List<Comment>>(okResult.Value);
        Assert.Equal(2, returnedComments.Count);
    }

    [Fact]
    public async Task GetCommentsByUser_ReturnsComments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Comments.AddRange(
            new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    BlogId = Guid.NewGuid(),
                    UserId = userId,
                    Content = "Test Comment 1",
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    BlogId = Guid.NewGuid(),
                    UserId = userId,
                    Content = "Test Comment 2",
                },
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetCommentsByUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedComments = Assert.IsType<List<Comment>>(okResult.Value);
        Assert.Equal(2, returnedComments.Count);
    }

    [Fact]
    public async Task AddComment_ValidComment_ReturnsCreatedComment()
    {
        // Arrange
        var blog = new Blog
        {
            Id = Guid.NewGuid(),
            Title = "Test Blog",
            Content = "Test Content",
            User = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "testuser@example.com",
                Password = "TestPassword123",
                Phone = "123-456-7890",
            },
        };
        _context.Add(blog);
        await _context.SaveChangesAsync();

        var newComment = new Comment
        {
            Id = Guid.NewGuid(),
            BlogId = blog.Id,
            Blog = blog,
            UserId = Guid.NewGuid(),
            Content = "New Comment",
        };

        // Act
        var result = await _controller.AddComment(newComment);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedComment = Assert.IsType<Comment>(createdResult.Value);
        Assert.Equal(newComment.Content, returnedComment.Content);
    }

    [Fact]
    public async Task UpdateComment_ValidId_UpdatesComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var existingComment = new Comment { Id = commentId, Content = "Old Content" };
        _context.Comments.Add(existingComment);
        await _context.SaveChangesAsync();

        var updatedComment = new Comment { Content = "Updated Content" };

        // Act
        var result = await _controller.UpdateComment(commentId, updatedComment);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Updated Content", existingComment.Content);
    }

    [Fact]
    public async Task DeleteComment_ValidId_DeletesComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var existingComment = new Comment
        {
            Id = commentId,
            Content = "Test Content",
            BlogId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };
        _context.Comments.Add(existingComment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteComment(commentId);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Null(await _context.Comments.FindAsync(commentId));
    }
}
