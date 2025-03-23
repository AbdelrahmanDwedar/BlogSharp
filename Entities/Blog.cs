namespace BlogSharp.Entities;

public class Blog
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }

    public int LikesCount { get; set; } = 0;
    public int DislikesCount { get; set; } = 0;

    public DateTime PublishDate { get; set; }
    public DateTime? UpdateDate { get; set; }

    public required User User { get; set; }
    public Guid UserId { get; set; }

    public ICollection<Comment>? Comments { get; set; }
}
