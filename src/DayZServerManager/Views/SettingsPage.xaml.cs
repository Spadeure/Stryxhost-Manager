using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DayZServerManager.Services;

namespace DayZServerManager.Views;

public partial class SettingsPage : UserControl
{
    private bool _ready;

    public SettingsPage()
    {
        InitializeComponent();
        LoadOptions();
        ShowSection("launch");
    }

    /// <summary>
    /// Cette page sert deux rubriques des Paramètres :
    /// « launch » (redémarrages et lancement) et « app » (langue, démarrage avec Windows, zone de notification).
    /// </summary>
    public void ShowSection(string section)
    {
        bool app = section == "app";
        TitleText.Text = app ? "APPLICATION" : "LANCEMENT";
        SubtitleText.Text = app
            ? "La langue de Stryxhost Manager, son lancement avec Windows et la zone de notification."
            : "Redémarrages automatiques du serveur et ligne de commande de lancement.";
        RestartCard.Visibility = app ? Visibility.Collapsed : Visibility.Visible;
        LanguageCard.Visibility = app ? Visibility.Visible : Visibility.Collapsed;
        StartupCard.Visibility = app ? Visibility.Visible : Visibility.Collapsed;
        TrayCard.Visibility = app ? Visibility.Visible : Visibility.Collapsed;
        StatusText.Text = "";
    }

    private void LoadOptions()
    {
        _ready = false;
        var settings = AppSettings.Current;
        AutoRestartToggle.IsChecked = settings.AutoRestart;
        ScheduledToggle.IsChecked = settings.ScheduledRestart;
        WarnToggle.IsChecked = settings.WarnBeforeRestart;
        HoursBox.Text = settings.RestartHours.ToString();
        AutoStartServerToggle.IsChecked = settings.AutoStartServer;
        CloseToTrayToggle.IsChecked = settings.CloseToTray;
        _ready = true;
    }

    /// <summary>Relit l'état réel (appelé à chaque ouverture de l'onglet).</summary>
    public void Refresh()
    {
        LoadOptions();
        try { StartWithWindowsToggle.IsChecked = StartupService.IsEnabled(); }
        catch { StartWithWindowsToggle.IsChecked = false; }
    }

    private void StartWithWindows_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (StartWithWindowsToggle.IsChecked == true)
            {
                StartupService.Enable();
                ShowStatus("Stryxhost Manager se lancera à l'ouverture de Windows, réduit près de l'horloge.", "CyanBrush");
            }
            else
            {
                StartupService.Disable();
                ShowStatus("Lancement avec Windows désactivé.", "MutedBrush");
            }
        }
        catch (Exception ex)
        {
            ShowStatus($"Erreur : {ex.Message}", "WarnBrush");
            Refresh();
        }
    }

    private void LaunchSettings_Click(object sender, RoutedEventArgs e)
    {
        var window = new LaunchWindow { Owner = Window.GetWindow(this) };
        if (window.ShowDialog() != true) return;
        ShowStatus(ServerManager.Instance.IsRunning
            ? "Paramètres de lancement enregistrés : ils seront appliqués au prochain redémarrage."
            : "Paramètres de lancement enregistrés.", "CyanBrush");
    }

    private void Options_Changed(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        var settings = AppSettings.Current;
        settings.AutoRestart = AutoRestartToggle.IsChecked == true;
        settings.ScheduledRestart = ScheduledToggle.IsChecked == true;
        settings.WarnBeforeRestart = WarnToggle.IsChecked == true;
        if (int.TryParse(HoursBox.Text.Trim(), out var hours) && hours >= 1 && hours <= 48)
            settings.RestartHours = hours;
        else
            HoursBox.Text = settings.RestartHours.ToString();
        settings.AutoStartServer = AutoStartServerToggle.IsChecked == true;
        settings.CloseToTray = CloseToTrayToggle.IsChecked == true;
        settings.Save();
        ShowStatus("Enregistré.", "CyanBrush");
    }

    private void ShowStatus(string message, string brushKey)
    {
        StatusText.Text = message;
        StatusText.Foreground = (Brush)Application.Current.FindResource(brushKey);
    }
}
