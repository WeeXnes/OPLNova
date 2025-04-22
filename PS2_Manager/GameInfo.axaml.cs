using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class GameInfo : UserControl
{
    public UpdateVar<Artwork.Type> ArtworkType { get; set; } = new UpdateVar<Artwork.Type>();
    public Game game { get; set; }
    public GameInfo(Game _game)
    {
        InitializeComponent();
        this.game = _game;
        ArtworkType.ValueChanged += UpdateArtworks;
        ArtworkType.Value = Artwork.Type.Front;
        
        GameNameTextBlock.Text = game.Name;
        GameIdTextBlock.Text = game.GameID;
        
        FileInfo fileInfo = new FileInfo(game.GamePath);
        IsoSizeTextBlock.Text = Util.FormatFileSize(fileInfo.Length);
        DateTime lastModified = fileInfo.LastWriteTime;
        string formattedDate = lastModified.ToString("dd.MM.yyyy HH:mm");
        GameDateTextBlock.Text = formattedDate;
    }



    private void UpdateArtworks()
    {
        
        Console.Info("Showing: " + ArtworkType.Value.ToString());
        Bitmap? tempBitmap = null;
        switch (ArtworkType.Value)
        {
            case Artwork.Type.Front:
                CoverContainer.Height = 292;
                tempBitmap = game.ArtworkFront;
                break;
            case Artwork.Type.Back:
                CoverContainer.Height = 292;
                tempBitmap = game.ArtworkBack;
                break;
            case Artwork.Type.Disc:
                CoverContainer.Height = CoverContainer.Width;
                tempBitmap = game.ArtworkDVD;
                break;
        }

        if (Util.IsBitmapEmpty(tempBitmap))
        {
            CoverContainer.Background = new SolidColorBrush(Color.Parse("#201c29"));
            CoverTextHint.IsVisible = true;
        }
        else
        {
            CoverContainer.Background = Brushes.Transparent;
            CoverTextHint.IsVisible = false;
        }
        


        CoverTextHint.Text = ArtworkType.Value.ToString();
  
        CoverImage.Source = tempBitmap;
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
            switch (ArtworkType.Value)
            {
                case Artwork.Type.Front:
                    this.game.ArtworkFront = new Bitmap(files[0].Path.LocalPath);
                    break;
                case Artwork.Type.Back:
                    this.game.ArtworkBack = new Bitmap(files[0].Path.LocalPath);
                    break;
                case Artwork.Type.Disc:
                    this.game.ArtworkDVD = new Bitmap(files[0].Path.LocalPath);
                    break;
            }
            Console.Error("Updating " + ArtworkType.Value + " Artwork for " + this.game.GameID);
            this.game.SaveCover(ArtworkType.Value);
            UpdateArtworks();
            MainWindow.RefreshGamesListTrigger?.Invoke(null, EventArgs.Empty);
        }
        else
        {
            Console.Error("OpenFilePicker was called but Canceled or no Files selected");
        }
    }

    private void NextButton(object? sender, RoutedEventArgs e)
    {
        this.ArtworkType.Value = Artwork.NextType(this.ArtworkType.Value);
    }

    private void PrevButton(object? sender, RoutedEventArgs e)
    {
        this.ArtworkType.Value = Artwork.PrevType(this.ArtworkType.Value);
    }
}