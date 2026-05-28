namespace JournalAI.API.DTOs;

public class CreateJournalDto
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Mood { get; set; } = string.Empty;
}