using System.Collections.Generic;
using System.Threading.Tasks;
using MarkdownNotesClient.Data;

namespace MarkdownNotesClient.Services;

public interface INoteService
{
    Task<IEnumerable<LocalNote>> GetLocalNotesAsync();
    Task<LocalNote?> GetNoteByIdAsync(int id);
    Task SaveNoteAsync(LocalNote note);
    Task DeleteNoteAsync(LocalNote noteToDelete);
    Task SyncWithCloudAsync();
}