using System.Windows;
using System.Windows.Controls;
using DayZServerManager.Themes;

namespace DayZServerManager.Views;

/// <summary>
/// Onglet ⚙ Paramètres : regroupe tout ce qui se règle une fois
/// (serveur, lancement, mises à jour, pare-feu, installation, application).
/// </summary>
public partial class SettingsHubPage : UserControl
{
    private readonly ConfigPage _config;
    private readonly SettingsPage _settings;
    private readonly UpdatePage _updates;
    private readonly FirewallPage _firewall;
    private readonly InstallPage _install;
    private string _current = "serveur";

    public SettingsHubPage(ConfigPage config, SettingsPage settings, UpdatePage updates, FirewallPage firewall, InstallPage install)
    {
        _config = config;
        _settings = settings;
        _updates = updates;
        _firewall = firewall;
        _install = install;
        InitializeComponent();
    }

    /// <summary>Rubrique ouverte (serveur, lancement, majs, parefeu, install, appli).</summary>
    public string Current => _current;

    /// <summary>Ouvre une rubrique.</summary>
    public void Select(string key)
    {
        var item = SubPanel.Children.OfType<RadioButton>().FirstOrDefault(r => (string)r.Tag == key);
        if (item == null) return;
        if (item.IsChecked == true) Show(key);
        else item.IsChecked = true; // déclenche Sub_Checked → Show
    }

    /// <summary>Relit la rubrique ouverte (appelé à chaque ouverture de l'onglet).</summary>
    public void Refresh()
    {
        if (!SubPanel.Children.OfType<RadioButton>().Any(r => r.IsChecked == true)) Select(_current);
        else Show(_current);
    }

    /// <summary>Petit compteur sur « Mises à jour » quand une nouvelle version de l'appli existe.</summary>
    public void SetUpdateBadge(bool available) =>
        NavProps.SetDescription(UpdatesSub, available ? "Nouvelle version de l'appli disponible !" : "Serveur DayZ et application");

    private void Sub_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string key }) Show(key);
    }

    private void Show(string key)
    {
        _current = key;
        switch (key)
        {
            case "serveur":
                SubHost.Content = _config;
                _config.Load();
                break;
            case "lancement":
                _settings.ShowSection("launch");
                SubHost.Content = _settings;
                _settings.Refresh();
                break;
            case "majs":
                SubHost.Content = _updates;
                _updates.RefreshServerOptions();
                break;
            case "parefeu":
                SubHost.Content = _firewall;
                _ = _firewall.RefreshAsync();
                break;
            case "install":
                SubHost.Content = _install;
                _install.Refresh();
                break;
            case "appli":
                _settings.ShowSection("app");
                SubHost.Content = _settings;
                _settings.Refresh();
                break;
        }
    }

    /// <summary>Fenêtre étroite : les rubriques n'affichent plus que leur icône.</summary>
    private void Hub_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        bool compact = ActualWidth < 980;
        SubColumn.Width = new GridLength(compact ? 58 : 236);
        GapColumn.Width = new GridLength(compact ? 20 : 28);
        HubTitle.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        foreach (var item in SubPanel.Children.OfType<RadioButton>()) NavProps.SetCompact(item, compact);
    }
}
