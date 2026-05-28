using JournalAI.API.Data;
using JournalAI.API.DTOs;
using JournalAI.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JournalAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JournalController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/journal
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var journals = await _context.JournalEntries
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return Ok(journals);
    }

    // POST: api/journal
    [HttpPost]
    public async Task<IActionResult> Create(CreateJournalDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var journal = new JournalEntry
        {
            Title = dto.Title,
            Content = dto.Content,
            Mood = dto.Mood,
            UserId = userId
        };

        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync();

        return Ok(journal);
    }

    // PUT: api/journal/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateJournalDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var journal = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (journal == null)
            return NotFound();

        journal.Title = dto.Title;
        journal.Content = dto.Content;
        journal.Mood = dto.Mood;

        await _context.SaveChangesAsync();

        return Ok(journal);
    }

    // DELETE: api/journal/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var journal = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        if (journal == null)
            return NotFound();

        _context.JournalEntries.Remove(journal);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Deleted successfully" });
    }
}