using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MarkdownNotesClient.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MarkdownNotesClient.Views;

public partial class NotesView : UserControl
{
    private ObservableCollection<LocalNote> _myNotes = new();

    public NotesView()
    {
        InitializeComponent();
        NotesList.ItemsSource = _myNotes;
    }

    public async void LoadNotes()
    {
        using var db = new LocalDbContext();
        var fetchedNotes = await db.Notes.Where(n => !n.IsDeleted).ToListAsync();

        _myNotes.Clear();
        foreach (var note in fetchedNotes)
        {
            _myNotes.Add(note);
        }
    }

    private async void OnCreateNoteClick(object sender, RoutedEventArgs e)
    {
        var newNote = new LocalNote
        {
            Title = "Untitled Note",
            Content = "Start typing your markdown here...",
            LastModified = DateTime.UtcNow,
            IsSynced = false
        };

        using var db = new LocalDbContext();
        db.Notes.Add(newNote);
        await db.SaveChangesAsync();

        _myNotes.Add(newNote);
        NotesList.SelectedItem = newNote;
        NoteTitleField.Text = newNote.Title;
        NoteContentField.Text = newNote.Content;
    }

    private async void OnSaveNoteClick(object sender, RoutedEventArgs e)
    {
        if (NotesList.SelectedItem is LocalNote selectedNote)
        {
            selectedNote.Title = NoteTitleField.Text ?? "Untitled";
            selectedNote.Content = NoteContentField.Text ?? "";
            selectedNote.LastModified = DateTime.UtcNow;
            selectedNote.IsSynced = false;

            using var db = new LocalDbContext();
            db.Notes.Update(selectedNote);
            await db.SaveChangesAsync();

            var index = _myNotes.IndexOf(selectedNote);
            if (index >= 0)
            {
                _myNotes.RemoveAt(index);
                _myNotes.Insert(index, selectedNote);
                NotesList.SelectedItem = selectedNote;
            }
        }
    }

    private async void OnDeleteNoteClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is LocalNote noteToDelete)
        {
            using var db = new LocalDbContext();

            noteToDelete.IsDeleted = true;
            noteToDelete.IsSynced = false;

            db.Notes.Update(noteToDelete);
            await db.SaveChangesAsync();

            _myNotes.Remove(noteToDelete);

            if (NotesList.SelectedItem == noteToDelete)
            {
                NoteTitleField.Text = "";
                NoteContentField.Text = "";
            }
        }
    }

    private void OnNoteSelected(object sender, SelectionChangedEventArgs e)
    {
        if (NotesList.SelectedItem is LocalNote selectedNote)
        {
            NoteTitleField.Text = selectedNote.Title;
            NoteContentField.Text = selectedNote.Content;
        }
    }

    private async void OnSyncClick(object sender, RoutedEventArgs e)
    {
        SyncButton.Content = "Syncing...";
        SyncButton.IsEnabled = false;

        var api = new MarkdownNotesClient.Services.ApiService();

        using var db = new LocalDbContext();
        var session = db.Sessions.FirstOrDefault();
        if (session != null)
        {
            api.SetToken(session.Token);
        }

        await api.SyncWithCloudAsync();
        LoadNotes();

        SyncButton.Content = "🔄 Sync to Cloud";
        SyncButton.IsEnabled = true;
    }
    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        using var db = new LocalDbContext();
        foreach (var session in db.Sessions) { db.Sessions.Remove(session); }
        foreach (var note in db.Notes) { db.Notes.Remove(note); }
        db.SaveChanges();
        var api = new MarkdownNotesClient.Services.ApiService();
        api.SetToken("");
        var window = this.GetVisualRoot() as Window;
        if (window != null)
        {
            window.Content = new MainView();
        }
    }
}