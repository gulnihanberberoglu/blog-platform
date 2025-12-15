namespace Blog.Api.Models;

public class Comment
{
    public int Id { get; set; }
    public required string Body { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int PostId { get; set; }
    public Post? Post { get; set; }

    public int AuthorId { get; set; }
    public User? Author { get; set; }
}
