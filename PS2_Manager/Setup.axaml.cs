using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using PS2_Manager.Core;

namespace PS2_Manager;

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
        settings.library_path.SetValue("");
        while (String.IsNullOrEmpty(settings.library_path.GetValue<string>()))
        {
            var topLevel = TopLevel.GetTopLevel(this);
            var path = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Game Library",
                AllowMultiple = false
            });
            try
            {
                if (!String.IsNullOrEmpty(path[0].Path.LocalPath))
                    settings.library_path.SetValue(path[0].Path.LocalPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        FinishSetup();
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if(!this.SetupFinished) e.Cancel = true;
    }
}