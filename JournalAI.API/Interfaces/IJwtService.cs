using JournalAI.API.Models;

namespace JournalAI.API.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
