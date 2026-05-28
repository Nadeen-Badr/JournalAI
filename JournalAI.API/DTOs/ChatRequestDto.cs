namespace JournalAI.API.DTOs;

public class ChatRequestDto
{
    public int JournalId { get; set; }

    public List<MessageDto> Messages { get; set; } = new();
}
