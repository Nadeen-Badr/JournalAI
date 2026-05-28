using System.Net.Http.Json;
using JournalAI.API.Data;
using JournalAI.API.DTOs;
using JournalAI.API.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
namespace JournalAI.API.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AiService(HttpClient httpClient, IConfiguration config, ApplicationDbContext context)
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
        var model = _config["Gemini:Model"];
    
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
- Help the user reflect on their thoughts and emotions
- Ask gentle follow-up questions when appropriate
- Keep responses short (max 6-8 lines)
- Stay natural, not robotic
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

        var contents = new List<object>
        {
            new
            {
                role = "user",
                parts = new[] { new { text = systemPrompt } }
            }
        };

        // limit context (IMPORTANT FIX)
        var lastMessages = messages
            .TakeLast(10)
            .ToList();

        foreach (var msg in lastMessages)
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

        return text;
    }
}