using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MarkdownNotesClient.Services;
using MarkdownNotesClient.Data; // For LocalNote
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MarkdownNotesClient.ViewModels;

public partial class NotesViewModel : ViewModelBase
{
    private readonly INoteService _noteService;

    public ObservableCollection<LocalNote> Notes { get; } = new();

    // ObservableProperty automatically tells the UI when SelectedNote changes!
    [ObservableProperty]
    private LocalNote? _selectedNote;

    public NotesViewModel(INoteService noteService)
    {
        _noteService = noteService;
    }

    public async Task LoadNotesAsync()
    {
        var fetchedNotes = await _noteService.GetLocalNotesAsync();
        Notes.Clear();
        foreach (var note in fetchedNotes)
        {
            Notes.Add(note);
        }
    }

    // CommunityToolkit generates "CreateNoteCommand" automatically from this method
    [RelayCommand]
    public async Task CreateNoteAsync()
    {
        var newNote = new LocalNote
        {
            Title = "Untitled Note",
            Content = "Start typing your markdown here...",
            LastModified = DateTime.UtcNow,
            IsSynced = false
        };

        await _noteService.SaveNoteAsync(newNote);
        Notes.Add(newNote);
        SelectedNote = newNote;
    }

    [RelayCommand]
    public async Task SaveNoteAsync()
    {
        if (SelectedNote != null)
        {
            SelectedNote.LastModified = DateTime.UtcNow;
            SelectedNote.IsSynced = false;

            await _noteService.SaveNoteAsync(SelectedNote);
            
            // Sync after saving
            await _noteService.SyncWithCloudAsync();
        }
    }

    [RelayCommand]
    public async Task DeleteNoteAsync(LocalNote noteToDelete)
    {
        if (noteToDelete != null)
        {
            await _noteService.DeleteNoteAsync(noteToDelete);
            Notes.Remove(noteToDelete);
            
            if (SelectedNote == noteToDelete)
            {
                SelectedNote = null;
            }
        }
    }

    public async Task SyncDatabaseAsync()
    {
        await _noteService.SyncWithCloudAsync();
        await LoadNotesAsync();
    }
}