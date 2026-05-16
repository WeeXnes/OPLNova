using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using OPLNova.Core;

namespace OPLNova;

public partial class BulkScraperWindow : Window
{
    private List<Game> _games;
    private StringBuilder _log = new StringBuilder();

    public BulkScraperWindow(List<Game> games)
    {
        InitializeComponent();
        _games = games;
        StatusTextBlock.Text = $"Ready to scrape artwork for {_games.Count} games.";
        ScrapeProgressBar.Maximum = _games.Count;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Start_Click(object? sender, RoutedEventArgs e)
    {
        StartButton.IsVisible = false;
        OverwriteCheckbox.IsEnabled = false;
        bool force = OverwriteCheckbox.IsChecked ?? false;
        
        int count = 0;
        foreach (var game in _games)
        {
            count++;
            Dispatcher.UIThread.Post(() => {
                StatusTextBlock.Text = $"Scraping {game.Name} ({count}/{_games.Count})...";
                ScrapeProgressBar.Value = count;
                AddToLog($"Processing: {game.Name} [{game.GameID}]");
            });

            await Task.Run(() => {
                try {
                    game.ScrapeArtwork(force);
                } catch (Exception ex) {
                    Dispatcher.UIThread.Post(() => AddToLog($"Error for {game.Name}: {ex.Message}"));
                }
            });
        }

        AddToLog("--- Done! ---");
        StatusTextBlock.Text = "Bulk scraping completed.";
        CloseButton.IsVisible = true;
        
        // Refresh the main window to show new art
        MainWindow.RefreshGamesListTrigger?.Invoke(this, EventArgs.Empty);
    }

    private void AddToLog(string message)
    {
        _log.AppendLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        LogTextBlock.Text = _log.ToString();
    }
}
