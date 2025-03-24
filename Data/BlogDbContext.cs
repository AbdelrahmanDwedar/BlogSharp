using BlogSharp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Data;

public class BlogDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options) { }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.isDeleted);
            entity
                .HasMany(user => user.Blogs)
                .WithOne(blog => blog.User)
                .HasForeignKey(blog => blog.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasMany(user => user.Comments)
                .WithOne(comment => comment.User)
                .HasForeignKey(comment => comment.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            if (Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            {
                entity
                    .HasIndex(blog => EF.Functions.ToTsVector("english", blog.Content))
                    .HasMethod("GIN");
            }
            entity.HasIndex(blog => blog.UserId);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasIndex(comment => comment.BlogId);
            entity.HasIndex(comment => comment.UserId);
        });
    }
}
