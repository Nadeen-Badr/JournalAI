namespace JournalAI.API.DTOs;

public class MessageDto
{
    public string Role { get; set; } = string.Empty; // "user" or "ai"

    public string Content { get; set; } = string.Empty;
}
