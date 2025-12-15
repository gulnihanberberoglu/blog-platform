namespace Blog.Api.Models;

public class Post
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int AuthorId { get; set; }
    public User? Author { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
