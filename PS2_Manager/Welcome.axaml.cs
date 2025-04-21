using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PS2_Manager;

public partial class Welcome : UserControl
{
    public Welcome()
    {
        InitializeComponent();
    }
    private void OnRepositoryLinkClicked(object sender, RoutedEventArgs e)
    {
        // Open the link in the default web browser
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://git.weexnes.dev/WeeXnes/ps2_manager",
            UseShellExecute = true
        });
    }
}