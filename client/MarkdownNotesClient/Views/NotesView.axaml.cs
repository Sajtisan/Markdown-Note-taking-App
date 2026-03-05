using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MarkdownNotesClient.Data;
using MarkdownNotesClient.ViewModels;
using MarkdownNotesClient.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MarkdownNotesClient.Views;

public partial class NotesView : UserControl
{
    // ---- Fields & Properties ----
    private DispatcherTimer _renderTimer;
    private bool _isSettingsOpen = false;
    private bool _isNotesSidebarOpen = true;
    private bool _isSyncingOnExit = false;

    private NotesViewModel? ViewModel => DataContext as NotesViewModel;

    // ---- Constructor & Initialization ----
    public NotesView()
    {
        InitializeComponent();

        if (App.Services != null)
        {
            DataContext = App.Services.GetRequiredService<NotesViewModel>();
        }

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
        if (ViewModel != null)
        {
            await ViewModel.SyncDatabaseAsync();
        }

        if (TopLevel.GetTopLevel(this) is Window parentWindow)
        {
            parentWindow.Closing += OnWindowClosing;
        }
    }

    private async void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isSyncingOnExit && ViewModel != null)
        {
            e.Cancel = true;
            _isSyncingOnExit = true;

            await ViewModel.SyncDatabaseAsync();

            if (sender is Window window)
            {
                window.Close();
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
        // Auto-close the sidebar when a note is selected on smaller screens
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

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        var api = new MarkdownNotesClient.Services.ApiService();
        api.SetToken("");

        if (this.GetVisualRoot() is Window window)
        {
            window.Content = new MainView(); // Swap back to login
        }
    }
}