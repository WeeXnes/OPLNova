using System;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PS2_Manager.Core;

namespace PS2_Manager;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Globals.LoadSettings();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (String.IsNullOrEmpty(settings.library_path.GetValue<string>()))
            {
                var setupWindow = new Setup();

                setupWindow.Closed += (_, _) =>
                {
                    // Replace MainWindow after setup finishes
                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        var mainWindow = new MainWindow();
                        desktop.MainWindow = mainWindow;
                        mainWindow.Show();
                    }
                };

                desktop.MainWindow = setupWindow;
            }
            else
            {
                desktop.MainWindow = new MainWindow();
            }

        }

        base.OnFrameworkInitializationCompleted();
    }
}