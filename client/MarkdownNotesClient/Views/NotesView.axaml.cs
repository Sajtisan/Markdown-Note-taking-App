using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Threading;
using Avalonia.Input;
using MarkdownNotesClient.Data;
using MarkdownNotesClient.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MarkdownNotesClient.Views;

public partial class NotesView : UserControl
{
    // ---- Fields & Properties ----
    private ObservableCollection<LocalNote> _myNotes = new();
    private DispatcherTimer _renderTimer;
    private bool _isSettingsOpen = false;
    private bool _isNotesSidebarOpen = true;
    private bool _isSyncingOnExit = false;

    // ---- Constructor & Initialization ----
    public NotesView()
    {
        InitializeComponent();
        NotesList.ItemsSource = _myNotes;
        DynamicCanvas.LoadShader(ThemeManager.Theme_Dark);
        
        _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _renderTimer.Tick += (s, e) =>
        {
            _renderTimer.Stop();
            MarkdownPreview.Markdown = NoteContentField.Text;
        };

        this.Loaded += OnNotesViewLoaded;
    }

    private async void OnNotesViewLoaded(object? sender, RoutedEventArgs e)
    {
        await SyncDatabaseAsync();

        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            parentWindow.Closing += OnWindowClosing;
        }
    }

    // ---- Note Management (Local) ----
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
        await SyncDatabaseAsync();
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

    // ---- Editor & UI State Logic ----
    private void OnContentChanged(object sender, TextChangedEventArgs e)
    {
        _renderTimer.Stop();
        _renderTimer.Start();
    }

    private void OnNoteSelected(object sender, SelectionChangedEventArgs e)
    {
        if (NotesList.SelectedItem is LocalNote selectedNote)
        {
            NoteTitleField.Text = selectedNote.Title;
            NoteContentField.Text = selectedNote.Content;
            NoteContentField.CaretIndex = 0;
        }
        
        if (_isNotesSidebarOpen)
        {
            _isNotesSidebarOpen = false;
            UpdateNotesSidebarState();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (_isSettingsOpen && SettingsDrawerBorder != null)
        {
            SettingsDrawerBorder.Height = e.NewSize.Height * 0.85;
        }
    }

    // ---- Sync & Auth Logic ----
    private async Task SyncDatabaseAsync()
    {
        var api = new ApiService();
        using var db = new LocalDbContext();
        var session = db.Sessions.FirstOrDefault();
        
        if (session != null)
        {
            api.SetToken(session.Token);
        }

        await api.SyncWithCloudAsync();
        LoadNotes();
        Console.WriteLine("Cloud Sync Complete!");
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        using var db = new LocalDbContext();
        foreach (var session in db.Sessions) { db.Sessions.Remove(session); }
        foreach (var note in db.Notes) { db.Notes.Remove(note); }
        db.SaveChanges();
        
        var api = new ApiService();
        api.SetToken("");
        
        if (this.GetVisualRoot() is Window window)
        {
            window.Content = new MainView();
        }
    }

    private async void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isSyncingOnExit)
        {
            e.Cancel = true;
            _isSyncingOnExit = true;
            await SyncDatabaseAsync();

            if (sender is Window window)
            {
                window.Close();
            }
        }
    }

    // ---- Settings Panel Logic ----
    private void OnSettingsToggleClick(object sender, RoutedEventArgs e)
    {
        _isSettingsOpen = !_isSettingsOpen;
        UpdateSettingsPanelState();
    }

    private void OnSettingsPointerExited(object sender, PointerEventArgs e)
    {
        if (_isSettingsOpen)
        {
            _isSettingsOpen = false;
            UpdateSettingsPanelState();
        }
    }

    private void UpdateSettingsPanelState()
    {
        if (_isSettingsOpen)
        {
            SettingsDrawerBorder.Height = this.Bounds.Height * 0.85;
            SettingsArrow.Classes.Remove("closed");
            SettingsArrow.Classes.Add("open");
        }
        else
        {
            SettingsDrawerBorder.Height = 0;
            SettingsArrow.Classes.Remove("open");
            SettingsArrow.Classes.Add("closed");
        }
    }

    // ---- Sidebar Logic ----
    private void OnNotesSidebarToggleClick(object sender, RoutedEventArgs e)
    {
        _isNotesSidebarOpen = !_isNotesSidebarOpen;
        UpdateNotesSidebarState();
    }

    private void UpdateNotesSidebarState()
    {
        if (_isNotesSidebarOpen)
        {
            NotesSidebarContainer.Classes.Remove("closed");
            NotesSidebarContainer.Classes.Add("open");
            NotesSidebarArrow.Classes.Remove("closed");
            NotesSidebarArrow.Classes.Add("open");
        }
        else
        {
            NotesSidebarContainer.Classes.Remove("open");
            NotesSidebarContainer.Classes.Add("closed");
            NotesSidebarArrow.Classes.Remove("open");
            NotesSidebarArrow.Classes.Add("closed");
        }
    }
}