using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class EditGame : UserControl
{
    public Game game { get; set; }
    public UpdateVar<string> MD5Hash { get; set; } = new UpdateVar<string>();
    public UpdateVar<string> SHA1Hash { get; set; } = new UpdateVar<string>();
    public EditGame(Game _game)
    {
        this.game = _game;
        InitializeComponent();
        DisplayNameBox.Text = this.game.Name;
        MD5Hash.ValueChanged += () =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Console.WriteLine("MD5Hash.ValueChanged");
                MD5TextBlock.Text = "MD5: " + MD5Hash.Value;
            });
        };
        SHA1Hash.ValueChanged += () =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Console.WriteLine("SHA1Hash.ValueChanged");
                SHATextBlock.Text = "SHA1: " + SHA1Hash.Value;
            });
        };
    }

    private void DisplayNameBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        
    }

    private void ChecksumButtonOnClick(object? sender, RoutedEventArgs e)
    {
        this.GetChecksum();
    }
    
    public void GetChecksum()
    {
        string filePath = this.game.GamePath;
        
        MD5TextBlock.Text = "MD5: Loading...";
        SHATextBlock.Text = "SHA1: Loading...";
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
        sha1Worker.RunWorkerAsync();

        Console.WriteLine("Hashing started...");
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        this.game.ChangeName(DisplayNameBox.Text);
        MainWindow.RefreshGamesListTrigger?.Invoke(null, EventArgs.Empty);
    }

    private void Edit_OnClick(object? sender, RoutedEventArgs e)
    {
        DisplayNameBox.Text = game.Name;
    }
}