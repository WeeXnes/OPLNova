using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class MainWindow : Window
{
    public static EventHandler? RefreshGamesListTrigger { get; set; }
    
    public UpdateVar<Artwork.Type> ArtworkType { get; set; } = new UpdateVar<Artwork.Type>();
    public UpdateVar<string> MD5Hash { get; set; } = new UpdateVar<string>();
    public UpdateVar<string> SHA1Hash { get; set; } = new UpdateVar<string>();
    
    public MainWindow()
    {
        InitializeComponent();
        RefreshGamesListTrigger += FetchGamesFromLibrary;
        ArtworkType.ValueChanged += UpdateArtworks;
        MD5Hash.ValueChanged += () =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Console.Info("MD5 Hash calculated");
                MD5TextBlock.Text = "MD5: " + MD5Hash.Value;
            });
        };
        SHA1Hash.ValueChanged += () =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Console.Info("SHA1 Hash calculated");
                SHATextBlock.Text = "SHA1: " + SHA1Hash.Value;
            });
        };
    }


    private void GamesList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            Console.Info("Selected " + selectedGame + " from ListView");
            SetEditArea();
            SetShowcaseArea();
            SwitchView(false);
        }
    }

    private void SwitchView(bool placeholder)
    {
        GameEdit.IsVisible = !placeholder;
        WelcomePanel.IsVisible = placeholder;
        
        GameInfo.IsVisible = !placeholder;
        GameInfoPlaceholder.IsVisible = placeholder;
    }

    private void SetShowcaseArea()
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            ArtworkType.Value = Artwork.Type.Front;
        
            GameNameTextBlock.Text = selectedGame.Name;
            GameIdTextBlock.Text = selectedGame.GameID;
            FileInfo fileInfo = new FileInfo(selectedGame.GamePath);
            IsoSizeTextBlock.Text = Util.FormatFileSize(fileInfo.Length);
            DateTime lastModified = fileInfo.LastWriteTime;
            string formattedDate = lastModified.ToString("dd.MM.yyyy HH:mm");
            GameDateTextBlock.Text = formattedDate;
        }
    }

    private void SetEditArea()
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            ChecksumArea.IsVisible = false;
            NameTextBox_Edit.Text = selectedGame.Name;
            VMC_1.SelectedItem = selectedGame.Config?.MemoryCard1;
            VMC_2.SelectedItem = selectedGame.Config?.MemoryCard2;
            toggle_mode1.IsChecked = selectedGame.Config?.Mode1;
            toggle_mode2.IsChecked = selectedGame.Config?.Mode2;
            toggle_mode3.IsChecked = selectedGame.Config?.Mode3;
            toggle_mode4.IsChecked = selectedGame.Config?.Mode4;
            toggle_mode5.IsChecked = selectedGame.Config?.Mode5;
            toggle_mode6.IsChecked = selectedGame.Config?.Mode6;
        }
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
        FetchGamesFromLibrary();
    }
    
    public void FetchGamesFromLibrary(object? sender = null, EventArgs? e = null)
    {
        SwitchView(true);
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
        FetchVMCs();
    }

    private void FetchVMCs()
    {
        List<string> VMCs = new List<string>();
        Util.CheckDir(Path.Combine(settings.library_path.GetValue<string>(), "VMC"));
        string[] files = Directory.GetFiles(Path.Combine(settings.library_path.GetValue<string>(), "VMC"));
        VMCs.Add("");
        foreach (var file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            VMCs.Add(fileName);
        }
        VMC_1.ItemsSource = VMCs;
        VMC_2.ItemsSource = VMCs;
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
    

    
 


    
    
    //Game Edit Area/////////////////////////////////////////////////////////////
    
    private void OnRepositoryLinkClicked(object sender, RoutedEventArgs e)
    {
        Console.Info("Repository Link Clicked");
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://git.weexnes.dev/WeeXnes/ps2_manager",
            UseShellExecute = true
        });
    }
    
    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            selectedGame.ChangeName(NameTextBox_Edit.Text ?? "Error");
            MainWindow.RefreshGamesListTrigger?.Invoke(null, EventArgs.Empty);
        }
    }

    private void Undo_OnClick(object? sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            NameTextBox_Edit.Text = selectedGame.Name;
        }
    }
    
        
    private void ChecksumButtonOnClick(object? sender, RoutedEventArgs e)
    {
        this.GetChecksum();
    }
    
    public void GetChecksum()
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {        
            string filePath = selectedGame.GamePath;
        
            this.MD5Hash.Value = "Loading...";
            this.SHA1Hash.Value = "Loading...";
            ChecksumArea.IsVisible = true;
        
            BackgroundWorker md5Worker = new BackgroundWorker();
            BackgroundWorker sha1Worker = new BackgroundWorker();
        
            md5Worker.DoWork += (s, e) =>
            {
                this.MD5Hash.Value = Checksum.GetMD5Hash(filePath);
            };

            sha1Worker.DoWork += (s, e) =>
            {
                this.SHA1Hash.Value = Checksum.GetSHA1Hash(filePath);
            };

            md5Worker.RunWorkerAsync();
            Console.Info("MD5 Hashing started...");
            sha1Worker.RunWorkerAsync();
            Console.Info("SHA1 Hashing started...");
        }

    }
    
    //Game Showcase Area/////////////////////////////////////////////////////////////
    
    
    
    
    private void UpdateArtworks()
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            Console.Info("Showing: " + ArtworkType.Value.ToString());
            Bitmap? tempBitmap = null;
            switch (ArtworkType.Value)
            {
                case Artwork.Type.Front:
                    CoverContainer.Height = 292;
                    tempBitmap = selectedGame.ArtworkFront;
                    break;
                case Artwork.Type.Back:
                    CoverContainer.Height = 292;
                    tempBitmap = selectedGame.ArtworkBack;
                    break;
                case Artwork.Type.Disc:
                    CoverContainer.Height = CoverContainer.Width;
                    tempBitmap = selectedGame.ArtworkDVD;
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
    }
    
    
    
    private async void CoverImage_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
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
                        selectedGame.ArtworkFront = new Bitmap(files[0].Path.LocalPath);
                        break;
                    case Artwork.Type.Back:
                        selectedGame.ArtworkBack = new Bitmap(files[0].Path.LocalPath);
                        break;
                    case Artwork.Type.Disc:
                        selectedGame.ArtworkDVD = new Bitmap(files[0].Path.LocalPath);
                        break;
                }
                Console.Info("Updating " + ArtworkType.Value + " Artwork for " + selectedGame.GameID);
                selectedGame.SaveCover(ArtworkType.Value);
                UpdateArtworks();
                MainWindow.RefreshGamesListTrigger?.Invoke(null, EventArgs.Empty);
            }
            else
            {
                Console.Error("OpenFilePicker was called but Canceled or no Files selected");
            }
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

    private void VMC_1_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (GamesList.SelectedItem is Game selectedGame)
        {
            if (VMC_1.SelectedItem is string selectedVMC)
            {
                Console.Warning("Setting " + selectedVMC + " as VMC1 for " + selectedGame.GameID);
                selectedGame.Config.MemoryCard1 = selectedVMC;
                selectedGame.Config.SaveConfig();
            }
        }
    }
    private void VMC_2_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        if (GamesList.SelectedItem is Game selectedGame)
        {
            if (VMC_2.SelectedItem is string selectedVMC)
            {
                Console.Warning("Setting " + selectedVMC + " as VMC2 for " + selectedGame.GameID);
                selectedGame.Config.MemoryCard2 = selectedVMC;
                selectedGame.Config.SaveConfig();
            }
        }
    }

    private void Toggle_mode_OnIsCheckedChanged(object sender, RoutedEventArgs e)
    {
        if (GamesList.SelectedItem is Game selectedGame)
        {
            if (sender is ToggleSwitch toggle)
            {
                switch (toggle.Name)
                {
                    case "toggle_mode1":
                        selectedGame.Config.Mode1 = toggle_mode1.IsChecked.Value;
                        Console.Warning("Toggled Mode 1 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode1);
                        break;
                    case "toggle_mode2":
                        selectedGame.Config.Mode2 = toggle_mode2.IsChecked.Value;
                        Console.Warning("Toggled Mode 2 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode2);
                        break;
                    case "toggle_mode3":
                        selectedGame.Config.Mode3 = toggle_mode3.IsChecked.Value;
                        Console.Warning("Toggled Mode 3 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode3);
                        break;
                    case "toggle_mode4":
                        selectedGame.Config.Mode4 = toggle_mode4.IsChecked.Value;
                        Console.Warning("Toggled Mode 4 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode4);
                        break;
                    case "toggle_mode5":
                        selectedGame.Config.Mode5 = toggle_mode5.IsChecked.Value;
                        Console.Warning("Toggled Mode 5 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode5);
                        break;
                    case "toggle_mode6":
                        selectedGame.Config.Mode6 = toggle_mode6.IsChecked.Value;
                        Console.Warning("Toggled Mode 6 for " + selectedGame.GameID + " to " + selectedGame.Config.Mode6);
                        break;
                }
            }
            selectedGame.Config.SaveConfig();
        }
        
    }
}