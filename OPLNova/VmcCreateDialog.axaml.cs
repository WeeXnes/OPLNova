using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OPLNova.Core;

namespace OPLNova;

public partial class VmcCreateDialog : Window
{
    public VmcCreateDialog()
    {
        InitializeComponent();
        SizeComboBox.SelectedIndex = 0;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Create_Click(object? sender, RoutedEventArgs e)
    {
        string name = VmcNameTextBox.Text;
        if (string.IsNullOrWhiteSpace(name)) return;

        int sizeMb = 8;
        switch (SizeComboBox.SelectedIndex)
        {
            case 1: sizeMb = 16; break;
            case 2: sizeMb = 32; break;
            case 3: sizeMb = 64; break;
        }

        string vmcPath = Path.Combine(settings.library_path.GetValue<string>(), "VMC", name + ".bin");
        
        try
        {
            Util.CreateVmcFile(vmcPath, sizeMb);
            Close(true);
        }
        catch (Exception ex)
        {
            // Simple error handling for now
            Close(false);
        }
    }
}
