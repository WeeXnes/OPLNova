using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using OPLNova.Core;

namespace OPLNova;

public partial class Setup : Window
{
    private bool SetupFinished { get; set; } = false;
    public Setup()
    {
        InitializeComponent();
    }

    private void WindowDrag(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void FinishSetup()
    {
        Core.Console.Success("Setup Complete");
        InfoText1.Text = "Setup Finished";
        InfoText2.Text = "You can now exit to the Main Application";
        OpenLibraryButton.Content = "Exit";
        OpenLibraryButton.Click -= OpenLibraryButton_OnClick;
        this.SetupFinished = true;
        OpenLibraryButton.Click += (sender, args) =>
        {
            this.Close();
        };
    }

    private async void OpenLibraryButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Core.Console.Info("Open Library Button Clicked");
        settings.library_path.SetValue("");
        var topLevel = TopLevel.GetTopLevel(this);
        var path = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Open Game Library",
            AllowMultiple = false
        });
        try
        {
            if (!String.IsNullOrEmpty(path[0].Path.LocalPath))
            {
                settings.library_path.SetValue(path[0].Path.LocalPath);
                FinishSetup();
            }
                    
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if(!this.SetupFinished) Environment.Exit(0);
    }
}