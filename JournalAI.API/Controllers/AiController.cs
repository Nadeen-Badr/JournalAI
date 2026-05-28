using JournalAI.API.Data;
using JournalAI.API.DTOs;
using JournalAI.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JournalAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;
    private readonly ApplicationDbContext _context;
    public AiController(ApplicationDbContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;

    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(ChatRequestDto dto)
    {
        var response = await _aiService.ChatAsync(dto.JournalId, dto.Messages);

        return Ok(new
        {
            response
        });
    }

[HttpGet("history/{journalId}")]
    public async Task<IActionResult> GetHistory(int journalId)
    {
        var messages = await _context.ChatMessages
            .Where(x => x.JournalId == journalId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new
            {
                role = x.Role,
                content = x.Content
            })
            .ToListAsync();

        return Ok(messages);
    }
}