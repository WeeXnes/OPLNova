using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class MainWindow : Window
{
    public MainWindow()
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

    private void WindowClose(object? sender, PointerPressedEventArgs e)
    {
        this.Close();
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
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
        if (String.IsNullOrEmpty(settings.library_path.GetValue<string>()))
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var path = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Game Library",
                AllowMultiple = false
            });
            Console.WriteLine(path[0].Path.LocalPath);
            if(!String.IsNullOrEmpty(path[0].Path.LocalPath))
                settings.library_path.SetValue(path[0].Path.LocalPath);
        }
    }
    
}