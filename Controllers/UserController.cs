using BlogSharp.Data;
using BlogSharp.Dtos;
using BlogSharp.Entities;
using BlogSharp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly BlogDbContext _context;
    private readonly IRedisCache _cache;

    public UserController(BlogDbContext context, IRedisCache cache)
    {
        _context = context;
        _cache = cache;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _context.Users.Where(u => !u.isDeleted).ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(Guid id)
    {
        var cacheKey = $"User_{id}";
        var user = await _cache.GetAsync<User>(cacheKey);

        if (user == null)
        {
            user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> AddNewUserAsync(CreateUser newUser)
    {
        if (newUser == null)
        {
            return BadRequest("Invalid user data.");
        }

        var user = new User
        {
            Name = newUser.Name,
            Email = newUser.Email,
            Password = newUser.Password,
            Phone = newUser.Phone,
            BirthDate = newUser.BirthDate,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUser updatedUser)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Name ??= updatedUser.Name;
        user.Email ??= updatedUser.Email;
        user.Password ??= updatedUser.Password;
        user.Phone ??= updatedUser.Phone;
        user.BirthDate ??= updatedUser.BirthDate;

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        var cacheKey = $"User_{id}";
        await _cache.RemoveAsync(cacheKey); // Invalidate cache

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        var cacheKey = $"User_{id}";
        await _cache.RemoveAsync(cacheKey); // Invalidate cache

        return NoContent();
    }

    [HttpPatch("{id}/soft-delete")]
    public async Task<IActionResult> SoftDeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.isDeleted = true;
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id.Equals(id));
    }
}
