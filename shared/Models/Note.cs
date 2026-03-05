namespace shared.Models;

public class Note
{
    public int Id { get; set; }
    public Guid ShareId { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; } = false;
    public int AuthorId { get; set; }
}