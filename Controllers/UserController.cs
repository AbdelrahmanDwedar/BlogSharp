using BlogSharp.Data;
using BlogSharp.Dtos;
using BlogSharp.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly BlogDbContext _context;

    public UserController(BlogDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
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
    public async Task<IActionResult> UpdateUser(int id, UpdateUser updatedUser)
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

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id.Equals(id));
    }
}
