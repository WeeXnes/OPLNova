using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class MainWindow : Window
{
    public static EventHandler? RefreshGamesListTrigger { get; set; }
    public MainWindow()
    {
        InitializeComponent();
        RefreshGamesListTrigger += FetchGamesFromLibrary;
    }

 

    private void ShowcaseGame(Game game)
    {
        GameShowcaseGrid.IsVisible = true;
        InfoWindow.Child = new GameInfo(game);
        GameEdit.Child = new EditGame(game);
        Console.Info(game + " set as Showcase");
    }
    
    private void WindowDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void WindowClose(object? sender, PointerPressedEventArgs e)
    {
        Console.Success("Window closing");
        this.Close();
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select PS2 Iso file to install",
            AllowMultiple = false,
            FileTypeFilter = new []
            {
                Globals.FileDialogTypes.PS2Game
            }
        });

        if (files.Count >= 1)
        {
            Console.Info("AddGameWindow was called for " + files[0].Path.LocalPath);
            new AddGameWindow(files[0].Path.LocalPath).Show();
        }
        else
        {
            Console.Error("OpenFilePicker was called but Canceled or no Files selected");
        }
    }

    private async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        WindowTitle.Text = "PS2 Games Manager";
        GameEdit.Child = new Welcome();
        FetchGamesFromLibrary();
    }
    
    public void FetchGamesFromLibrary(object? sender = null, EventArgs? e = null)
    {
        Console.Info("Fetching games from library...");
        List<Game> Games = new List<Game>();
        Util.CheckDir(Path.Combine(settings.library_path.GetValue<string>(), "DVD"));
        string[] files = Directory.GetFiles(Path.Combine(settings.library_path.GetValue<string>(), "DVD"));
        foreach (var file in files)
        {
            Game newGame =
                new Game(file, true);
            Games.Add(newGame);
            Console.Success("Successfully fetched " + newGame);
        }
        Games = Games.OrderBy(game => game.Name).ToList();
        GamesList.ItemsSource = Games;
    }

    private void GamesList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            Console.Info("Selected " + selectedGame + " from ListView");
            this.ShowcaseGame(selectedGame);
        }
    }
    private void DeleteButton(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            Console.Warning("Uninstalling " + selectedGame);
            selectedGame.Uninstall();
            RefreshGamesListTrigger?.Invoke(sender, e);
        }
    }

    private void Context_RefreshGames(object? sender, RoutedEventArgs e)
    {
        FetchGamesFromLibrary();
    }

}