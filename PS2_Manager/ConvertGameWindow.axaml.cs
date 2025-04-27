using System.ComponentModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PS2_Manager.Core.IO.DiscImage;
using PS2_Manager.Core.IO.DiscImage.Type;

namespace PS2_Manager;

public partial class ConvertGameWindow : Window
{
    public bool PreventClose { get; set; } = false;
    public string binPath { get; set; }
    public string isoPath { get; set; }
    public BackgroundWorker Worker { get; set; }
    public ConvertGameWindow(string path)
    {
        this.binPath = path;
        this.isoPath = Path.ChangeExtension(path, ".iso");
        InitializeComponent();
        Worker = new BackgroundWorker();
        Worker.RunWorkerCompleted += WorkerOnRunWorkerCompleted;
        Worker.DoWork += WorkerOnDoWork;
    }

    private void WorkerOnDoWork(object? sender, DoWorkEventArgs e)
    {
        Console.Info("Worker has started");
        Iso9660Conv conv = new Iso9660Conv();
        conv.DiscImage = new BinType(this.binPath);
        conv.Convert(this.isoPath);
    }

    private void WorkerOnRunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        Console.Info("File Converted");
        OpenInstallWindow();
    }

    public void OpenInstallWindow()
    {
        Console.Success("Converted Successfully");
        this.PreventClose = false;
        new AddGameWindow(this.isoPath).Show();
        this.Close();
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e)
    {
        this.Label_1.Text = Path.GetFileName(this.binPath) + " needs to be converted into a ISO file";
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if(this.PreventClose)
            e.Cancel = true;
    }

    private void ConvertButton_OnClick(object? sender, RoutedEventArgs e)
    {
        this.PreventClose = true;
        ConvertButton.Content = "Converting...";
        Console.Warning("Converting " + this.binPath + " to ISO");
        Worker.RunWorkerAsync();
    }
}