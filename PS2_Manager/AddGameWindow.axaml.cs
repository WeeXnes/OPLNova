using System;
using System.Collections.Generic;
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
        InstallationMethodBox.Items.Add("Move");
        InstallationMethodBox.Items.Add("Copy");
        InstallationMethodBox.SelectedItem = settings.preferred_installation_method.GetValue<string>() ?? "Move";
    }

    private void RefreshPreview()
    {
        Console.Info("Refreshing AddGameWindow Preview");
        IsoPathBox.Text = this.newGame.GamePath;
        SerialBox.Text = this.newGame.GameID;
        GameNameBox.Text = this.newGame.Name;
        CoverImage.Source = this.newGame.ArtworkFront;
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        RefreshPreview();
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
            Console.WriteLine("Installation for " + newGame + " finished");
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
            Console.Info("Updating " + Artwork.Type.Front + " Artwork for " + this.newGame.GameID);
            this.newGame.ArtworkFront = new Bitmap(files[0].Path.LocalPath);
            RefreshPreview();
        }
        else
        {
            Console.Error("OpenFilePicker was called but Canceled or no Files selected");
        }
    }

    private void InstallationMethodBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        settings.preferred_installation_method.SetValue(InstallationMethodBox.SelectionBoxItem?.ToString());
    }
}