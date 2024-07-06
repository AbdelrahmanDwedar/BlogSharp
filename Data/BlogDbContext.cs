using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
            : base(options)
        {
        }

		// TODO: add sets for the entities
    }
}
