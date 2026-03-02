using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using MarkdownNotesClient.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MarkdownNotesClient.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _token;
    public string? GetToken() => _token;
    public ApiService()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5080/") };
    }
    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }
    public async Task<string> LoginAsync(string username, string password)
    {
        try
        {
            // 1. Using the absolute URL
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5080/api/login", new { username, password });

            if (response.IsSuccessStatusCode)
            {
                var rawJson = await response.Content.ReadAsStringAsync();

                // 2. Safely parse the JSON, checking both uppercase and lowercase
                using var jsonDoc = System.Text.Json.JsonDocument.Parse(rawJson);
                string? token = null;

                if (jsonDoc.RootElement.TryGetProperty("token", out var t1)) token = t1.GetString();
                else if (jsonDoc.RootElement.TryGetProperty("Token", out var t2)) token = t2.GetString();

                if (!string.IsNullOrEmpty(token))
                {
                    _token = token;
                    SetToken(_token);
                    return "SUCCESS"; // We are in!
                }

                return $"JSON Parse Failed. Server sent: {rawJson}";
            }

            return $"Server Rejected: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            // 3. Return the exact crash message!
            return $"Network Crash: {ex.Message}";
        }
    }
    public async Task<List<NoteDto>> GetMyNotesAsync()
    {
        try
        {
            var notes = await _httpClient.GetFromJsonAsync<List<NoteDto>>("api/notes/my");
            return notes ?? new List<NoteDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching notes: {ex.Message}");
            return new List<NoteDto>();
        }
    }
    public async Task<NoteDto?> CreateNoteAsync(string title, string content)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/notes", new { title, content });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NoteDto>();
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
    public async Task<bool> UpdateNoteAsync(NoteDto note)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/notes/{note.Id}", note);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    public async Task<bool> DeleteNoteAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/notes/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    public async Task SyncWithCloudAsync()
    {
        using var db = new LocalDbContext();
        var unsyncedNotes = await db.Notes.Where(n => !n.IsSynced).ToListAsync();
        foreach (var note in unsyncedNotes)
        {
            try
            {
                if (note.IsDeleted && note.RemoteId.HasValue)
                {
                    var response = await _httpClient.DeleteAsync($"api/notes/{note.RemoteId}");
                    if (response.IsSuccessStatusCode) db.Notes.Remove(note);
                }
                else if (!note.IsDeleted && note.RemoteId == null)
                {
                    var response = await _httpClient.PostAsJsonAsync("api/notes", new { title = note.Title, content = note.Content });
                    if (response.IsSuccessStatusCode)
                    {
                        var cloudNote = await response.Content.ReadFromJsonAsync<NoteDto>();
                        if (cloudNote != null)
                        {
                            note.RemoteId = cloudNote.Id;
                            note.IsSynced = true;
                        }
                    }
                }
                else if (!note.IsDeleted && note.RemoteId.HasValue)
                {
                    var noteDto = new NoteDto { Id = note.RemoteId.Value, Title = note.Title, Content = note.Content };
                    var response = await _httpClient.PutAsJsonAsync($"api/notes/{note.RemoteId}", noteDto);
                    if (response.IsSuccessStatusCode) note.IsSynced = true;
                }
            }
            catch { /* Network drop */ }
        }

        await db.SaveChangesAsync();
        try
        {
            var cloudNotes = await GetMyNotesAsync();

            if (cloudNotes != null && cloudNotes.Any())
            {
                foreach (var cloudNote in cloudNotes)
                {
                    var localNote = await db.Notes.FirstOrDefaultAsync(n => n.RemoteId == cloudNote.Id);

                    if (localNote == null)
                    {
                        db.Notes.Add(new LocalNote
                        {
                            RemoteId = cloudNote.Id,
                            Title = cloudNote.Title,
                            Content = cloudNote.Content,
                            LastModified = DateTime.UtcNow,
                            IsSynced = true,
                            IsDeleted = false
                        });
                    }
                    else if (localNote.IsSynced)
                    {
                        localNote.Title = cloudNote.Title;
                        localNote.Content = cloudNote.Content;
                    }
                }
                await db.SaveChangesAsync();
            }
        }
        catch { /* Network drop during pull */ }
    }
}

public class LoginResponse { public string Token { get; set; } = ""; }
public class NoteDto //Data transfer object - DTO
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }

}