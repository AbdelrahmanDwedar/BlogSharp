using BlogSharp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Data;

public class BlogDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Id);
        });
    }
}
