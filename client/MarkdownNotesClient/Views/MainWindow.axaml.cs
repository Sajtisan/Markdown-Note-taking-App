using Avalonia.Controls;
using MarkdownNotesClient.Data;
using MarkdownNotesClient.Services;
using System.Linq;

namespace MarkdownNotesClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        using var db = new LocalDbContext();
        db.Database.EnsureCreated();
        var savedSession = db.Sessions.FirstOrDefault();
        
        if (savedSession != null && !string.IsNullOrEmpty(savedSession.Token))
        {
            var api = new ApiService();
            api.SetToken(savedSession.Token);
            var notesView = new NotesView(); 
            this.Content = notesView;
            notesView.LoadNotes();
        }
        else
        {
            this.Content = new MainView();
        }
    }
}