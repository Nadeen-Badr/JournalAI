using JournalAI.API.DTOs;
using JournalAI.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JournalAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiService _aiService;

    public AiController(IAiService aiService)
    {
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
}
