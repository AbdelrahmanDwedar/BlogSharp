namespace BlogSharp.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; }

    public int LinksCount { get; set; }
    public int DislikesCount { get; set; }

    public Blog Blog { get; set; }
    public Guid BlogId { get; set; }

    public User User { get; set; }
    public Guid UserId { get; set; }

    public Comment(Blog blog, User user, string content)
    {
        this.Blog = blog;
        this.User = user;
        this.Content = content;
    }
}
