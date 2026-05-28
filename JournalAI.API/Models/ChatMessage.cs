namespace JournalAI.API.Models;

public class ChatMessage
{
    public int Id { get; set; }

    public int JournalId { get; set; }
    public JournalEntry Journal { get; set; }

    public string Role { get; set; } // "user" or "ai"
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}