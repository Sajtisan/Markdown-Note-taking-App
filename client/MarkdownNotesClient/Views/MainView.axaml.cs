using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using MarkdownNotesClient.Services;
using MarkdownNotesClient.Data;

namespace MarkdownNotesClient.Views;

public partial class MainView : UserControl
{
    private int _currentThemeIndex = 1;
    public MainView()
    {
        InitializeComponent();
        DynamicCanvas.LoadShader(ThemeManager.Theme_Dark);
        ThemeCycleButton.Content = "🎨 Theme: Dark";
    }

    private void RegisterOrLoginClick(object sender, RoutedEventArgs e)
    {
        if (RegOrLogButton.Content?.ToString() == "Register")
        {
            RegOrLogButton.Content = "Back to Login";
            LoginPanel.IsVisible = false;
            RegisterPanel.IsVisible = true;
        }
        else
        {
            RegOrLogButton.Content = "Register";
            LoginPanel.IsVisible = true;
            RegisterPanel.IsVisible = false;
        }
        LoginErrorText.IsVisible = false;
        RegErrorText.IsVisible = false;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        var api = new MarkdownNotesClient.Services.ApiService();

        string loginResult = await api.LoginAsync(LoginUsernameField.Text ?? "", LoginPasswordField.Text ?? "");

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
            LoginErrorText.Text = loginResult;
            LoginErrorText.Foreground = Avalonia.Media.Brushes.Red;
            LoginErrorText.IsVisible = true;
        }
    }

    private async void OnRegisterClick(object sender, RoutedEventArgs e)
    {
        var api = new MarkdownNotesClient.Services.ApiService();
        string regResult = await api.RegisterAsync(RegUsernameField.Text ?? "", RegPasswordField.Text ?? "");
        if (regResult == "SUCCESS")
        {
            RegErrorText.Text = "Success! You can now log in.";
            RegErrorText.Foreground = Avalonia.Media.Brushes.LightGreen;
            RegErrorText.IsVisible = true;
            RegUsernameField.Text = "";
            RegPasswordField.Text = "";
        }
        else
        {
            RegErrorText.Text = regResult;
            RegErrorText.Foreground = Avalonia.Media.Brushes.Red;
            RegErrorText.IsVisible = true;
        }
    }
    // 1. The Fog Toggle Method
    private void OnFogToggleClick(object sender, RoutedEventArgs e)
    {
        if (FogToggle.IsChecked == true)
        {
            DynamicCanvas.IsVisible = true;
            FogToggle.Content = "🌫️ FX: ON";
        }
        else
        {
            DynamicCanvas.IsVisible = false;
            FogToggle.Content = "🌫️ FX: OFF";
        }
    }

    // 2. The Theme Cycle Method
    private void OnThemeCycleClick(object sender, RoutedEventArgs e)
    {
        _currentThemeIndex++;
        if (_currentThemeIndex > 3) _currentThemeIndex = 1;

        switch (_currentThemeIndex)
        {
            case 1:
                DynamicCanvas.LoadShader(MarkdownNotesClient.Services.ThemeManager.Theme_Dark);
                ThemeCycleButton.Content = "🎨 Theme: Dark";
                // Force Avalonia to Dark Mode UI
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                break;
            case 2:
                DynamicCanvas.LoadShader(MarkdownNotesClient.Services.ThemeManager.Theme_Dracula);
                ThemeCycleButton.Content = "🎨 Theme: Dracula";
                // Force Avalonia to Dark Mode UI (Dracula is a dark theme)
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                break;
            case 3:
                DynamicCanvas.LoadShader(MarkdownNotesClient.Services.ThemeManager.Theme_Light);
                ThemeCycleButton.Content = "🎨 Theme: Light";
                // Force Avalonia to Light Mode UI (This will turn your text and buttons dark!)
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                break;
        }
    }
}