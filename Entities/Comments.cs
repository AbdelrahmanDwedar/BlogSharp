namespace BlogSharp.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public string Content { get; set; }

    public int LinksCount { get; set; }
    public int DislikesCount { get; set; }

    public Blog Blog { get; set; }

    public Comment(Blog blog, string content)
    {
        this.Blog = blog;
        this.Content = content;
    }
}
