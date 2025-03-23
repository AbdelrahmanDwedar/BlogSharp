using BlogSharp.Data;
using BlogSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlogController : ControllerBase
{
	private readonly BlogDbContext _context;

	public BlogController(BlogDbContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Blog>>> GetAllBlogs()
	{
		var blogs = await _context.Blogs.Include(b => b.User).ToListAsync();
		return Ok(blogs);
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Blog>> GetBlogById(Guid id)
	{
		var blog = await _context.Blogs.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);

		if (blog == null)
		{
			return NotFound();
		}

		return Ok(blog);
	}

	[HttpPost]
	public async Task<ActionResult<Blog>> AddNewBlog(Blog newBlog)
	{
		if (newBlog == null)
		{
			return BadRequest("Invalid blog data.");
		}

		newBlog.PublishDate = DateTime.UtcNow;
		_context.Blogs.Add(newBlog);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetBlogById), new { id = newBlog.Id }, newBlog);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateBlog(Guid id, Blog updatedBlog)
	{
		var blog = await _context.Blogs.FindAsync(id);
		if (blog == null)
		{
			return NotFound();
		}

		blog.Title = updatedBlog.Title ?? blog.Title;
		blog.Content = updatedBlog.Content ?? blog.Content;
		blog.UpdateDate = DateTime.UtcNow;

		_context.Entry(blog).State = EntityState.Modified;
		await _context.SaveChangesAsync();

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteBlog(Guid id)
	{
		var blog = await _context.Blogs.FindAsync(id);
		if (blog == null)
		{
			return NotFound();
		}

		_context.Blogs.Remove(blog);
		await _context.SaveChangesAsync();

		return NoContent();
	}

	[HttpGet("search")]
	public async Task<ActionResult<IEnumerable<Blog>>> SearchBlogs(string query)
	{
		var blogs = await _context.Blogs
			.Where(b => EF.Functions.ToTsVector("english", b.Content).Matches(query))
			.ToListAsync();

		return Ok(blogs);
	}
}
