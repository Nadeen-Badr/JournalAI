using JournalAI.API.Data;
using JournalAI.API.DTOs;
using JournalAI.API.Interfaces;
using JournalAI.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;
namespace JournalAI.API.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AiService(
        HttpClient httpClient,
        IConfiguration config,
        ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _config = config;
        _context = context;
    }

    public async Task<string> ChatAsync(int journalId, List<MessageDto> messages)
    {
        var journal = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.Id == journalId);

        if (journal == null)
            return "Journal not found";

        var apiKey = _config["Gemini:ApiKey"];

        var url =
            $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var moodInstruction = journal.Mood switch
        {
            "Sad" => "The user seems emotionally low. Be extra gentle and supportive.",
            "Stressed" => "The user is stressed. Help reduce pressure and simplify thoughts.",
            "Happy" => "The user is in a positive state. Reinforce positivity.",
            _ => "Respond in a balanced and reflective tone."
        };

        var systemPrompt =
$"""
You are a supportive AI journaling assistant inside a personal journaling app.

{moodInstruction}

Your role:
- Help the user reflect on thoughts and emotions
- Ask gentle follow-up questions
- Keep responses short (max 6-8 lines)
- Stay natural and conversational
- Focus ONLY on the journal context

Journal Entry:
Title: {journal.Title}
Content: {journal.Content}
Mood: {journal.Mood}

Rules:
- Do NOT invent facts
- Do NOT change topic outside journal context
- Be empathetic and calm
""";

        // =========================
        // LOAD SAVED CHAT HISTORY
        // =========================

        var history = await _context.ChatMessages
        .Where(x => x.JournalId == journalId)
        .OrderByDescending(x => x.CreatedAt)
        .Take(20)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();

        // =========================
        // SAVE NEW USER MESSAGE
        // =========================

        var latestUserMessage = messages.LastOrDefault(x => x.Role == "user");

        if (latestUserMessage != null)
        {
            _context.ChatMessages.Add(new ChatMessage
            {
                JournalId = journalId,
                Role = "user",
                Content = latestUserMessage.Content
            });

            await _context.SaveChangesAsync();
        }

        // =========================
        // BUILD AI CONTEXT
        // =========================

        var contents = new List<object>
        {
            new
            {
                role = "user",
                parts = new[]
                {
                    new { text = systemPrompt }
                }
            }
        };

        foreach (var msg in history)
        {
            contents.Add(new
            {
                role = msg.Role == "user" ? "user" : "model",
                parts = new[]
                {
                    new { text = msg.Content }
                }
            });
        }

        // include latest frontend messages too
        var recentMessages = messages.TakeLast(10);

        foreach (var msg in recentMessages)
        {
            contents.Add(new
            {
                role = msg.Role == "user" ? "user" : "model",
                parts = new[]
                {
                    new { text = msg.Content }
                }
            });
        }

        var requestBody = new
        {
            contents
        };

        HttpResponseMessage response;

        try
        {
            response = await _httpClient.PostAsJsonAsync(url, requestBody);
        }
        catch
        {
            return "AI service is unreachable.";
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();

            return $"AI request failed: {response.StatusCode} - {errorText}";
        }

        // =========================
        // PARSE GEMINI RESPONSE
        // =========================

        var jsonString = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonString);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
            return "AI returned empty response.";

        // =========================
        // SAVE AI MESSAGE
        // =========================

        _context.ChatMessages.Add(new ChatMessage
        {
            JournalId = journalId,
            Role = "ai",
            Content = text
        });

        await _context.SaveChangesAsync();

        return text;
    }
}