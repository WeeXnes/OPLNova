using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OPLNova.Core;
using Tmds.DBus.Protocol;

namespace OPLNova;

public partial class App : Application
{
    
    private void SetExceptionHandler()
    {
        AppDomain currentDomain = default(AppDomain);
        currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
    }
    private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex = (Exception)e.ExceptionObject;
        Core.Console.Error("Exception: " + ex.Message);
        DateTime currentDateTime = DateTime.Now;
        string formattedDateTime = currentDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss");
        using (StreamWriter writer = new StreamWriter("error_log.txt", append: true))
        {
            writer.WriteLine(formattedDateTime);
            writer.WriteLine(ex.ToString());
            writer.WriteLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }
    }
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        SetExceptionHandler();
        if(!Path.Exists(Globals.UserPath))
            Directory.CreateDirectory(Globals.UserPath);
        Console.SetLogFile(Globals.LogPath);
        Globals.LoadSettings();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (String.IsNullOrEmpty(settings.library_path.GetValue<string>()))
            {
                var setupWindow = new Setup();

                setupWindow.Closed += (_, _) =>
                {
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