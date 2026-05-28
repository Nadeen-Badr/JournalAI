using JournalAI.API.DTOs;

namespace JournalAI.API.Interfaces;

public interface IAiService
{
    Task<string> ChatAsync(int journalId, List<MessageDto> messages);
}
