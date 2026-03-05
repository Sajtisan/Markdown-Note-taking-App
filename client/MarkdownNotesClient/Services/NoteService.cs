using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarkdownNotesClient.Data; // For LocalDbContext and LocalNote
using shared.Models;
using Microsoft.EntityFrameworkCore;

namespace MarkdownNotesClient.Services;

public class NoteService : INoteService
{
    private readonly ApiService _apiService;

    public NoteService()
    {
        _apiService = new ApiService(); 
    }

    public async Task<IEnumerable<LocalNote>> GetLocalNotesAsync()
    {
        using var db = new LocalDbContext();
        return await db.Notes.Where(n => !n.IsDeleted).ToListAsync();
    }

    public async Task<LocalNote?> GetNoteByIdAsync(int id)
    {
        using var db = new LocalDbContext();
        return await db.Notes.FindAsync(id);
    }

    public async Task SaveNoteAsync(LocalNote note)
    {
        using var db = new LocalDbContext();
        if (note.Id == 0) // New Note
        {
            db.Notes.Add(note);
        }
        else // Existing Note
        {
            db.Notes.Update(note);
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteNoteAsync(LocalNote noteToDelete)
    {
        using var db = new LocalDbContext();
        noteToDelete.IsDeleted = true;
        noteToDelete.IsSynced = false;

        db.Notes.Update(noteToDelete);
        await db.SaveChangesAsync();
    }

    public async Task SyncWithCloudAsync()
    {
        using var db = new LocalDbContext();
        var session = db.Sessions.FirstOrDefault();
        
        if (session != null)
        {
            _apiService.SetToken(session.Token);
        }

        await _apiService.SyncWithCloudAsync();
        Console.WriteLine("Cloud Sync Complete!");
    }
}