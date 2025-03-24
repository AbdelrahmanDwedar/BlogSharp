using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Entities;

public class Comment
{
    public Guid Id { get; set; }

    [Required]
    public Guid BlogId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(500)]
    public string Content { get; set; }

    public Blog Blog { get; set; }
    public User User { get; set; }

    // Parameterless constructor for EF Core
    public Comment() { }

    // Optional constructor for convenience
    public Comment(Blog blog, User user, string content)
    {
        Blog = blog;
        User = user;
        Content = content;
        BlogId = blog.Id;
        UserId = user.Id;
    }
}
