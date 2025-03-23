using Microsoft.AspNetCore.Identity;

namespace BlogSharp.Entities;

public class User : IdentityUser<Guid>
{
    public new Guid Id { get; set; }
    public required string Name { get; set; }
    public new required string Email { get; set; }
    public required string Password { get; set; }
    public required string Phone { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateOnly? BirthDate { get; set; }

    public bool isDeleted { get; set; } = false;

    public ICollection<Blog>? Blogs { get; set; }
    public ICollection<Comment>? Comments { get; set; }
}
