using System.Collections.Generic;
using System.Threading.Tasks;
using shared.Models;

namespace MarkdownNotesClient.Services;

public class NoteService : INoteService
{
    public NoteService()
    {
    }

    public async Task<IEnumerable<Note>> GetLocalNotesAsync()
    {
        
        return new List<Note>();
    }

    public async Task<Note?> GetNoteByIdAsync(int id)
    {
        
        return null;
    }

    public async Task SaveNoteAsync(Note note)
    {
        
    }

    public async Task DeleteNoteAsync(int id)
    {
        
    }

    public async Task SyncWithCloudAsync()
    {
        
    }
}