using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Input;
using MarkdownNotesClient.Services;
using MarkdownNotesClient.Controls;
using MarkdownNotesClient.Data;
using System;

namespace MarkdownNotesClient.Views;

public partial class MainView : UserControl
{
    // ---- Fields & Properties ----
    private int _currentThemeIndex = 1;
    private bool _isSettingsOpen = false;
    private bool _isAppearanceOpen = false;

    // ---- Constructor & Initialization ----
    public MainView()
    {
        InitializeComponent();
        DynamicCanvas.LoadShader(ThemeManager.Theme_Dark);
        ThemeCycleButton.Content = "🎨 Theme: Dark";
    }

    // ---- Authentication Logic (Login/Register) ----
    private void RegisterOrLoginClick(object sender, RoutedEventArgs e)
    {
        if (RegOrLogButton.Content?.ToString() == "Register")
        {
            RegOrLogButton.Content = "Back to Login";

            // Fade out Login, Fade in Register
            LoginPanel.Opacity = 0;
            LoginPanel.IsHitTestVisible = false;

            RegisterPanel.Opacity = 1;
            RegisterPanel.IsHitTestVisible = true;
        }
        else
        {
            RegOrLogButton.Content = "Register";

            // Fade out Register, Fade in Login
            RegisterPanel.Opacity = 0;
            RegisterPanel.IsHitTestVisible = false;

            LoginPanel.Opacity = 1;
            LoginPanel.IsHitTestVisible = true;
        }
        LoginErrorText.IsVisible = false;
        RegErrorText.IsVisible = false;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        var api = new ApiService();

        string loginResult = await api.LoginAsync(LoginUsernameField.Text ?? "", LoginPasswordField.Text ?? "");

        if (loginResult == "SUCCESS")
        {
            using var db = new LocalDbContext();

            foreach (var session in db.Sessions) { db.Sessions.Remove(session); }
            db.Sessions.Add(new LocalSession { Token = api.GetToken() ?? "" });
            db.SaveChanges();

            if (this.GetVisualRoot() is Window window)
            {
                var notesView = new NotesView();
                window.Content = notesView;
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
        var api = new ApiService();
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

    // ---- Appearance & Theme Logic ----
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

    private void OnThemeCycleClick(object sender, RoutedEventArgs e)
    {
        _currentThemeIndex++;
        if (_currentThemeIndex > 3) _currentThemeIndex = 1;

        switch (_currentThemeIndex)
        {
            case 1:
                DynamicCanvas.LoadShader(ThemeManager.Theme_Dark);
                ThemeCycleButton.Content = "🎨 Theme: Dark";
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                break;
            case 2:
                DynamicCanvas.LoadShader(ThemeManager.Theme_Dracula);
                ThemeCycleButton.Content = "🎨 Theme: Dracula";
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
                break;
            case 3:
                DynamicCanvas.LoadShader(ThemeManager.Theme_Light);
                ThemeCycleButton.Content = "🎨 Theme: Light";
                Avalonia.Application.Current!.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
                break;
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
            SettingsPanelContainer.Classes.Remove("closed");
            SettingsPanelContainer.Classes.Add("open");
            SettingsArrow.Classes.Remove("closed");
            SettingsArrow.Classes.Add("open");
        }
        else
        {
            SettingsPanelContainer.Classes.Remove("open");
            SettingsPanelContainer.Classes.Add("closed");
            SettingsArrow.Classes.Remove("open");
            SettingsArrow.Classes.Add("closed");
        }
    }

    private void OnAppearanceToggleClick(object sender, RoutedEventArgs e)
    {
        _isAppearanceOpen = !_isAppearanceOpen;

        if (_isAppearanceOpen)
        {
            AppearanceArrow.Classes.Remove("closed");
            AppearanceArrow.Classes.Add("open");
            AppearanceText.Classes.Remove("closed");
            AppearanceText.Classes.Add("open");
            AppearanceContent.Classes.Remove("closed");
            AppearanceContent.Classes.Add("open");
        }
        else
        {
            AppearanceArrow.Classes.Remove("open");
            AppearanceArrow.Classes.Add("closed");
            AppearanceText.Classes.Remove("open");
            AppearanceText.Classes.Add("closed");
            AppearanceContent.Classes.Remove("open");
            AppearanceContent.Classes.Add("closed");
        }
    }
}