using BlogSharp.Data;
using BlogSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController : ControllerBase
{
	private readonly BlogDbContext _context;

	public CommentController(BlogDbContext context)
	{
		_context = context;
	}

	[HttpGet("blog/{blogId}")]
	public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByBlog(Guid blogId)
	{
		var comments = await _context.Comments
			.Where(c => c.Blog.Id == blogId)
			.ToListAsync();

		return Ok(comments);
	}

	[HttpGet("user/{userId}")]
	public async Task<ActionResult<IEnumerable<Comment>>> GetCommentsByUser(Guid userId)
	{
		var comments = await _context.Comments
			.Where(c => c.UserId == userId)
			.ToListAsync();

		return Ok(comments);
	}

	[HttpPost]
	public async Task<ActionResult<Comment>> AddComment(Comment newComment)
	{
		if (newComment == null || newComment.Blog == null)
		{
			return BadRequest("Invalid comment data.");
		}

		_context.Comments.Add(newComment);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetCommentsByBlog), new { blogId = newComment.Blog.Id }, newComment);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateComment(Guid id, Comment updatedComment)
	{
		var comment = await _context.Comments.FindAsync(id);
		if (comment == null)
		{
			return NotFound();
		}

		comment.Content = updatedComment.Content ?? comment.Content;

		_context.Entry(comment).State = EntityState.Modified;
		await _context.SaveChangesAsync();

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteComment(Guid id)
	{
		var comment = await _context.Comments.FindAsync(id);
		if (comment == null)
		{
			return NotFound();
		}

		_context.Comments.Remove(comment);
		await _context.SaveChangesAsync();

		return NoContent();
	}
}
