using System.Collections.Generic;
using System.Threading.Tasks;
using shared.Models; 

namespace MarkdownNotesClient.Services;

public interface INoteService
{
    Task<IEnumerable<Note>> GetLocalNotesAsync();
    Task<Note?> GetNoteByIdAsync(int id);
    Task SaveNoteAsync(Note note);
    Task DeleteNoteAsync(int id);
    Task SyncWithCloudAsync(); 
}