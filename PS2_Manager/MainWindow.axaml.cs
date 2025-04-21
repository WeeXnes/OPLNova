using System;
using System.Collections.Generic;
using System.IO;
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
            //Console.WriteLine(ISO.GetSerial());
            new AddGameWindow(files[0].Path.LocalPath).Show();
        }
    }

    private async void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        WindowTitle.Text = "PS2 Games Manager";
        FetchGamesFromLibrary();
    }
    
    public void FetchGamesFromLibrary(object? sender = null, EventArgs? e = null)
    {
        List<Game> Games = new List<Game>();
        Console.WriteLine("Loading library...");
        string[] files = Directory.GetFiles(Path.Combine(settings.library_path.GetValue<string>(), "DVD"));
        foreach (var file in files)
        {
            Console.WriteLine(file);
            Game newGame =
                new Game(file, true);
            Games.Add(newGame);
        }
        GamesList.ItemsSource = Games;
    }

    private void GamesList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            Console.WriteLine($"Clicked on game: {selectedGame.Name}, ID: {selectedGame.GameID}");
            this.ShowcaseGame(selectedGame);
        }
    }

    private void Context_RefreshGames(object? sender, RoutedEventArgs e)
    {
        FetchGamesFromLibrary();
    }
}