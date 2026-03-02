using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MarkdownNotesClient.Services;
using MarkdownNotesClient.Data;

namespace MarkdownNotesClient.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        var api = new MarkdownNotesClient.Services.ApiService();

        // Capture the string result instead of a bool
        string loginResult = await api.LoginAsync(UsernameField.Text ?? "", PasswordField.Text ?? "");

        if (loginResult == "SUCCESS")
        {
            using var db = new MarkdownNotesClient.Data.LocalDbContext();

            foreach (var session in db.Sessions) { db.Sessions.Remove(session); }
            db.Sessions.Add(new MarkdownNotesClient.Data.LocalSession { Token = api.GetToken() ?? "" });
            db.SaveChanges();

            var window = this.GetVisualRoot() as Avalonia.Controls.Window;
            if (window != null)
            {
                var notesView = new NotesView();
                window.Content = notesView;
                notesView.LoadNotes();
            }
        }
        else
        {
            // Display the EXACT error right on the UI!
            ErrorText.Text = loginResult;
            ErrorText.Foreground = Avalonia.Media.Brushes.Red;
            ErrorText.IsVisible = true;
        }
    }
}