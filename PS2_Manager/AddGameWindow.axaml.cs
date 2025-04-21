using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class AddGameWindow : Window
{
    public string Gamepath { get; set; }
    public Game newGame { get; set; }
    public AddGameWindow(string filePath)
    {
        this.newGame = new Game(filePath);
        InitializeComponent();
    }

    private void RefreshPreview()
    {
        IsoPathBox.Text = this.newGame.GamePath;
        SerialBox.Text = this.newGame.GameID;
        GameNameBox.Text = this.newGame.Name;
        CoverImage.Source = this.newGame.ArtworkFront;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        RefreshPreview();
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var newSize = e.NewSize;
        Console.WriteLine($"[SizeChanged] New size: {newSize.Width} x {newSize.Height}");
    }
    
    public void LoadPs2Cover(string gameId, Image imageControl)
    {
        //gameId = "SCPS-15110";
        string url = $"https://github.com/xlenore/ps2-covers/blob/main/covers/default/{gameId}.jpg?raw=true";

        try
        {
            using HttpClient client = new();
            byte[] data = client.GetByteArrayAsync(url).GetAwaiter().GetResult();

            using MemoryStream stream = new(data);
            var bitmap = new Bitmap(stream);

            imageControl.Source = bitmap;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Could not load cover for {gameId}: {ex.Message}");
        }
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

    private void InstallButton_OnClick(object? sender, RoutedEventArgs e)
    {
        InstallButton.IsEnabled = false;
        GameNameBox.IsEnabled = false;
        InstallButton.Content = "Installing...";
        newGame.Install();
        newGame.InstallProgress.ValueChanged += () =>
        {
            InstallationProgressBar.Value = newGame.InstallProgress.Value;
        };
        newGame.InstallationFinished += (o, args) =>
        {
            Console.WriteLine("Installation finished");
            this.Close();
        };
    }

    private void GameNameBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        this.newGame.Name = GameNameBox.Text ?? "";
    }

    private async void CoverImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a Cover for the PS2 Game",
            AllowMultiple = false,
            FileTypeFilter = new []
            {
                Globals.FileDialogTypes.PS2Cover
            }
        });

        if (files.Count >= 1)
        {
            this.newGame.ArtworkFront = new Bitmap(files[0].Path.LocalPath);
            RefreshPreview();
        }
    }
}